using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Authorization;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using ResponseData = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.ResponseData;

namespace Stargazer.Orleans.ObjectStorage.Silo.Controllers;

/// <summary>
/// 对象存储控制器
/// 提供对象的 CRUD 操作、分片上传及签名 URL 生成
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/storage/object")]
[Authorize]
public class ObjectController(IClusterClient client, ILogger<ObjectController> logger) : ControllerBase
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
    /// 检查当前用户对存储桶的访问权限
    /// </summary>
    private async Task<bool> HasBucketAccessAsync(Guid bucketId, string action, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = client.GetGrain<IBucketGrain>(0);
        return await bucketGrain.HasAccessPermissionAsync(bucketId, userId, action, cancellationToken);
    }

    /// <summary>
    /// 下载对象
    /// </summary>
    [HttpGet("{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.View}")]
    public async Task<IActionResult> DownloadObject(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Read, cancellationToken);
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        var stream = await objectGrain.DownloadAsync(bucketId, key, cancellationToken);

        if (stream == null)
        {
            throw new KeyNotFoundException("object_not_found");
        }

        var metadata = await objectGrain.GetMetadataAsync(bucketId, key, cancellationToken);

        return File(stream, metadata?.ContentType ?? "application/octet-stream", metadata?.FileName ?? key);
    }

    /// <summary>
    /// 检查对象是否存在
    /// </summary>
    [HttpHead("{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.View}")]
    public async Task CheckObjectExists(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Read, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        var exists = await objectGrain.ExistsAsync(bucketId, key, cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException("object_not_found");
        }
    }

    /// <summary>
    /// 获取对象元数据
    /// </summary>
    [HttpGet("metadata/{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObjectMetadataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<ObjectMetadataDto> GetObjectMetadata(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Read, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        var metadata = await objectGrain.GetMetadataAsync(bucketId, key, cancellationToken);

        if (metadata == null)
        {
            throw new KeyNotFoundException("object_not_found");
        }

        return metadata;
    }

    /// <summary>
    /// 列出存储桶中的对象
    /// </summary>
    [HttpGet("{bucketId:guid}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<ObjectMetadataDto>))]
    public async Task<PageResult<ObjectMetadataDto>> ListObjects(Guid bucketId, [FromQuery] string? prefix, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Read, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        return await objectGrain.ListObjectsAsync(bucketId, prefix, pageIndex, pageSize, cancellationToken);
    }

    /// <summary>
    /// 上传对象
    /// </summary>
    [HttpPost("{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadResultDto))]
    public async Task<UploadResultDto> UploadObject(Guid bucketId, string key, [FromForm] IFormFile file, [FromForm] string? contentType, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("invalid_file");
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = client.GetGrain<IObjectGrain>(0);

        return await objectGrain.UploadAsync(
            bucketId,
            key,
            stream,
            contentType ?? file.ContentType ?? "application/octet-stream",
            null,
            cancellationToken);
    }

    /// <summary>
    /// 更新/覆盖对象
    /// </summary>
    [HttpPut("{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadResultDto))]
    public async Task<UploadResultDto> UpdateObject(Guid bucketId, string key, [FromForm] IFormFile file, [FromForm] string? contentType, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("invalid_file");
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = client.GetGrain<IObjectGrain>(0);

        return await objectGrain.UploadAsync(
            bucketId,
            key,
            stream,
            contentType ?? file.ContentType ?? "application/octet-stream",
            null,
            cancellationToken);
    }

    /// <summary>
    /// 删除对象
    /// </summary>
    [HttpDelete("{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task DeleteObject(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        var result = await objectGrain.DeleteAsync(bucketId, key, cancellationToken);

        if (!result)
        {
            throw new KeyNotFoundException("object_not_found");
        }
    }

    /// <summary>
    /// 获取对象签名 URL
    /// </summary>
    [HttpGet("signed-url/{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SignedUrlDto))]
    public async Task<SignedUrlDto> GetSignedUrl(Guid bucketId, string key, [FromQuery] TimeSpan expiry, [FromQuery] string method = "GET", CancellationToken cancellationToken = default)
    {
        if (expiry.TotalSeconds > 604800)
        {
            throw new ArgumentException("invalid_expiry");
        }

        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Read, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        return await objectGrain.GetSignedUrlAsync(bucketId, key, expiry, method, cancellationToken);
    }

    /// <summary>
    /// 初始化分片上传
    /// </summary>
    [HttpPost("multipart/initiate/{bucketId:guid}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InitiateMultipartUploadResultDto))]
    public async Task<InitiateMultipartUploadResultDto> InitiateMultipartUpload(Guid bucketId, string key, [FromBody] InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        return await objectGrain.InitiateMultipartUploadAsync(bucketId, key, request.ContentType, request.Metadata, cancellationToken);
    }

    /// <summary>
    /// 上传分片
    /// </summary>
    [HttpPost("multipart/part/{bucketId:guid}/{uploadId}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadPartResultDto))]
    public async Task<UploadPartResultDto> UploadPart(Guid bucketId, string key, string uploadId, [FromForm] IFormFile file, [FromForm] int partNumber, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("invalid_file");
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = client.GetGrain<IObjectGrain>(0);

        return await objectGrain.UploadPartAsync(bucketId, key, uploadId, partNumber, stream, cancellationToken);
    }

    /// <summary>
    /// 完成分片上传
    /// </summary>
    [HttpPost("multipart/complete/{bucketId:guid}/{uploadId}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadResultDto))]
    public async Task<UploadResultDto> CompleteMultipartUpload(Guid bucketId, string key, string uploadId, [FromBody] CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        return await objectGrain.CompleteMultipartUploadAsync(bucketId, key, uploadId, request.Parts, cancellationToken);
    }

    /// <summary>
    /// 取消分片上传
    /// </summary>
    [HttpDelete("multipart/{bucketId:guid}/{uploadId}/{*key}")]
    [Authorize(policy: $"permission:{StoragePolicies.Objects.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task AbortMultipartUpload(Guid bucketId, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, StorageActions.Write, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("access_denied");
        }

        var objectGrain = client.GetGrain<IObjectGrain>(0);
        await objectGrain.AbortMultipartUploadAsync(bucketId, key, uploadId, cancellationToken);
    }
}

/// <summary>
/// 初始化分片上传请求
/// </summary>
public class InitiateMultipartUploadRequest
{
    public string ContentType { get; set; } = "application/octet-stream";

    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// 完成分片上传请求
/// </summary>
public class CompleteMultipartUploadRequest
{
    public List<PartETagDto> Parts { get; set; } = new();
}
