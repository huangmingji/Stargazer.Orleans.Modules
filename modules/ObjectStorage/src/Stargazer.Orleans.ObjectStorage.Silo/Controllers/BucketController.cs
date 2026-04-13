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
    /// <returns>当前用户 GUID</returns>
    /// <exception cref="UnauthorizedAccessException">Token 无效时抛出</exception>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }
        return userId;
    }

    /// <summary>
    /// 获取指定 ID 的存储桶
    /// </summary>
    /// <param name="id">存储桶 GUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储桶信息</returns>
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    public async Task<IActionResult> GetBucket(Guid id, CancellationToken cancellationToken = default)
    {
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var bucket = await bucketGrain.GetBucketAsync(id, cancellationToken);
        
        if (bucket == null)
        {
            return NotFound(ResponseData.Fail(code: "bucket_not_found", message: "Bucket not found."));
        }
        
        return Ok(ResponseData.Success(data: bucket));
    }

    /// <summary>
    /// 根据名称获取存储桶
    /// </summary>
    /// <param name="name">存储桶名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储桶信息</returns>
    [HttpGet("name/{name}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    public async Task<IActionResult> GetBucketByName(string name, CancellationToken cancellationToken = default)
    {
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var bucket = await bucketGrain.GetBucketByNameAsync(name, cancellationToken);
        
        if (bucket == null)
        {
            return NotFound(ResponseData.Fail(code: "bucket_not_found", message: "Bucket not found."));
        }
        
        return Ok(ResponseData.Success(data: bucket));
    }

    /// <summary>
    /// 获取当前用户的所有存储桶
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储桶列表</returns>
    [HttpGet]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    public async Task<IActionResult> GetUserBuckets(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var buckets = await bucketGrain.GetUserBucketsAsync(userId, cancellationToken);
        return Ok(ResponseData.Success(data: buckets));
    }

    /// <summary>
    /// 创建新的存储桶
    /// </summary>
    /// <param name="bucket">存储桶信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的存储桶信息</returns>
    [HttpPost]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Create}")]
    public async Task<IActionResult> CreateBucket([FromBody] BucketDto bucket, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            bucket.OwnerId = userId;
            var bucketGrain = client.GetGrain<IBucketGrain>(0);
            var created = await bucketGrain.CreateBucketAsync(bucket, cancellationToken);
            return CreatedAtAction(nameof(GetBucket), new { id = created.Id }, ResponseData.Success(data: created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseData.Fail(code: "bucket_exists", message: ex.Message));
        }
    }

    /// <summary>
    /// 更新存储桶信息
    /// </summary>
    /// <param name="id">存储桶 GUID</param>
    /// <param name="bucket">新的存储桶信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新后的存储桶信息</returns>
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Update}")]
    public async Task<IActionResult> UpdateBucket(Guid id, [FromBody] BucketDto bucket, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        
        var hasAccess = await bucketGrain.HasAccessPermissionAsync(id, userId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            return Forbid();
        }

        try
        {
            var updated = await bucketGrain.UpdateBucketAsync(id, bucket, cancellationToken);
            return Ok(ResponseData.Success(data: updated));
        }
        catch (InvalidOperationException)
        {
            return NotFound(ResponseData.Fail(code: "bucket_not_found", message: "Bucket not found."));
        }
    }

    /// <summary>
    /// 删除存储桶
    /// 注意：只有存储桶所有者才能删除，且存储桶必须为空
    /// </summary>
    /// <param name="id">存储桶 GUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.Delete}")]
    public async Task<IActionResult> DeleteBucket(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        
        var isOwner = await bucketGrain.IsOwnerAsync(id, userId, cancellationToken);
        if (!isOwner)
        {
            return Forbid();
        }

        try
        {
            var result = await bucketGrain.DeleteBucketAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(ResponseData.Fail(code: "bucket_not_found", message: "Bucket not found."));
            }
            return Ok(ResponseData.Success(data: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseData.Fail(code: "bucket_not_empty", message: ex.Message));
        }
    }

    /// <summary>
    /// 检查当前用户对存储桶的访问权限
    /// </summary>
    /// <param name="id">存储桶 GUID</param>
    /// <param name="action">操作类型 (Read/Write)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否有权限</returns>
    [HttpGet("{id:guid}/access")]
    [Authorize(policy: $"permission:{StoragePolicies.Buckets.View}")]
    public async Task<IActionResult> CheckAccess(Guid id, [FromQuery] string action, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        var hasAccess = await bucketGrain.HasAccessPermissionAsync(id, userId, action, cancellationToken);
        return Ok(ResponseData.Success(data: hasAccess));
    }
}
