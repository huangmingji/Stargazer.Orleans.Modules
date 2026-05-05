using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Authorization;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using ResponseData = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.ResponseData;

namespace Stargazer.Orleans.ObjectStorage.Silo.Controllers;

/// <summary>
/// 存储桶控制器
/// 提供存储桶的 CRUD 操作及权限管理
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/storage/bucket")]
[Authorize]
public class BucketController(IClusterClient client, ILogger<BucketController> logger) : ControllerBase
{
    /// <summary>
    /// 从 JWT Token 中获取当前用户 ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("invalid_token");
        }
        return userId;
    }

    /// <summary>
    /// 获取指定 ID 的存储桶
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BucketDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<BucketDto> GetBucket(Guid id, CancellationToken cancellationToken = default)
    {
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var bucket = await bucketGrain.GetBucketAsync(id, cancellationToken);

        if (bucket == null)
        {
            throw new KeyNotFoundException("bucket_not_found");
        }

        return bucket;
    }

    /// <summary>
    /// 根据名称获取存储桶
    /// </summary>
    [HttpGet("name/{name}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BucketDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<BucketDto> GetBucketByName(string name, CancellationToken cancellationToken = default)
    {
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var bucket = await bucketGrain.GetBucketByNameAsync(name, cancellationToken);

        if (bucket == null)
        {
            throw new KeyNotFoundException("bucket_not_found");
        }

        return bucket;
    }

    /// <summary>
    /// 获取当前用户的所有存储桶
    /// </summary>
    [HttpGet]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BucketDto>))]
    public async Task<List<BucketDto>> GetUserBuckets(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        return await bucketGrain.GetUserBucketsAsync(userId, cancellationToken);
    }

    /// <summary>
    /// 创建新的存储桶
    /// </summary>
    [HttpPost]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BucketDto))]
    public async Task<BucketDto> CreateBucket([FromBody] BucketDto bucket, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        bucket.OwnerId = userId;

        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        return await bucketGrain.CreateBucketAsync(bucket, cancellationToken);
    }

    /// <summary>
    /// 更新存储桶信息
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BucketDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<BucketDto> UpdateBucket(Guid id, [FromBody] BucketDto input, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);

        var bucket = await bucketGrain.GetBucketAsync(id, cancellationToken);
        if (bucket == null)
        {
            throw new KeyNotFoundException("bucket_not_found");
        }

        var hasAccess = await bucketGrain.HasAccessPermissionAsync(id, userId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        return await bucketGrain.UpdateBucketAsync(id, input, cancellationToken);
    }

    /// <summary>
    /// 删除存储桶
    /// 注意：只有存储桶所有者才能删除，且存储桶必须为空
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task DeleteBucket(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);

        var bucket = await bucketGrain.GetBucketAsync(id, cancellationToken);
        if (bucket == null)
        {
            throw new KeyNotFoundException("bucket_not_found");
        }

        var isOwner = await bucketGrain.IsOwnerAsync(id, userId, cancellationToken);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        await bucketGrain.DeleteBucketAsync(id, cancellationToken);
    }

    /// <summary>
    /// 检查当前用户对存储桶的访问权限
    /// </summary>
    [HttpGet("{id:guid}/access")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<bool> CheckAccess(Guid id, [FromQuery] string action, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        return await bucketGrain.HasAccessPermissionAsync(id, userId, action, cancellationToken);
    }
}
