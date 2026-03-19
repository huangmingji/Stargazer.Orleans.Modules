using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using ResponseData = Stargazer.Orleans.Users.Grains.Abstractions.ResponseData;

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
    private readonly IClusterClient _client = client;

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
    /// 检查当前用户对存储桶的访问权限
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="action">操作类型 (Read/Write)</param>
    /// <returns>是否有权限</returns>
    private async Task<bool> HasBucketAccessAsync(Guid bucketId, string action)
    {
        var userId = GetCurrentUserId();
        var bucketGrain = _client.GetGrain<IBucketGrain>(0);
        return await bucketGrain.HasAccessPermissionAsync(bucketId, userId, action);
    }

    /// <summary>
    /// 下载对象
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    [HttpGet("{bucketId:guid}/{*key}")]
    [Authorize(policy: "permission:storage.object.view")]
    public async Task<IActionResult> DownloadObject(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Read");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var stream = await objectGrain.DownloadAsync(bucketId, key, cancellationToken);
        
        if (stream == null)
        {
            return NotFound(ResponseData.Fail(code: "object_not_found", message: "Object not found."));
        }

        var metadata = await objectGrain.GetMetadataAsync(bucketId, key, cancellationToken);
        
        return File(stream, metadata?.ContentType ?? "application/octet-stream", metadata?.FileName ?? key);
    }

    /// <summary>
    /// 检查对象是否存在
    /// 使用 HEAD 请求，只返回响应头，不返回 body
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>204 表示存在，404 表示不存在</returns>
    [HttpHead("{bucketId:guid}/{*key}")]
    [Authorize(policy: "permission:storage.object.view")]
    public async Task<IActionResult> CheckObjectExists(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Read");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var exists = await objectGrain.ExistsAsync(bucketId, key, cancellationToken);
        
        if (!exists)
        {
            return NotFound();
        }
        
        return Ok();
    }

    /// <summary>
    /// 获取对象元数据
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>对象元数据</returns>
    [HttpGet("{bucketId:guid}/{*key}/metadata")]
    [Authorize(policy: "permission:storage.object.view")]
    public async Task<IActionResult> GetObjectMetadata(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Read");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var metadata = await objectGrain.GetMetadataAsync(bucketId, key, cancellationToken);
        
        if (metadata == null)
        {
            return NotFound(ResponseData.Fail(code: "object_not_found", message: "Object not found."));
        }
        
        return Ok(ResponseData.Success(data: metadata));
    }

    /// <summary>
    /// 列出存储桶中的对象
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="prefix">对象键前缀过滤</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>对象列表</returns>
    [HttpGet("{bucketId:guid}")]
    [Authorize(policy: "permission:storage.object.view")]
    public async Task<IActionResult> ListObjects(Guid bucketId, [FromQuery] string? prefix, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Read");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var objects = await objectGrain.ListObjectsAsync(bucketId, prefix, cancellationToken);
        return Ok(ResponseData.Success(data: objects));
    }

    /// <summary>
    /// 上传对象
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="file">上传的文件</param>
    /// <param name="contentType">文件 Content-Type</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    [HttpPost("{bucketId:guid}/{*key}")]
    [Authorize(policy: "permission:storage.object.create")]
    public async Task<IActionResult> UploadObject(Guid bucketId, string key, [FromForm] IFormFile file, [FromForm] string? contentType, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_file", message: "File is required."));
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        
        var result = await objectGrain.UploadAsync(
            bucketId, 
            key, 
            stream, 
            contentType ?? file.ContentType ?? "application/octet-stream",
            null,
            cancellationToken);
        
        return CreatedAtAction(nameof(DownloadObject), new { bucketId, key }, ResponseData.Success(data: result));
    }

    /// <summary>
    /// 更新/覆盖对象
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="file">上传的文件</param>
    /// <param name="contentType">文件 Content-Type</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    [HttpPut("{bucketId:guid}/{*key}")]
    [Authorize(policy: "permission:storage.object.update")]
    public async Task<IActionResult> UpdateObject(Guid bucketId, string key, [FromForm] IFormFile file, [FromForm] string? contentType, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_file", message: "File is required."));
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        
        var result = await objectGrain.UploadAsync(
            bucketId, 
            key, 
            stream, 
            contentType ?? file.ContentType ?? "application/octet-stream",
            null,
            cancellationToken);
        
        return Ok(ResponseData.Success(data: result));
    }

    /// <summary>
    /// 删除对象
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{bucketId:guid}/{*key}")]
    [Authorize(policy: "permission:storage.object.delete")]
    public async Task<IActionResult> DeleteObject(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var result = await objectGrain.DeleteAsync(bucketId, key, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "object_not_found", message: "Object not found."));
        }
        
        return Ok(ResponseData.Success(data: true));
    }

    /// <summary>
    /// 获取对象签名 URL
    /// 用于临时授权访问私有对象，最长有效期为 7 天
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="expiry">过期时间</param>
    /// <param name="method">HTTP 方法 (GET/PUT/DELETE 等)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>签名 URL</returns>
    [HttpGet("{bucketId:guid}/{*key}/signed-url")]
    [Authorize(policy: "permission:storage.object.view")]
    public async Task<IActionResult> GetSignedUrl(Guid bucketId, string key, [FromQuery] TimeSpan expiry, [FromQuery] string method = "GET", CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Read");
        if (!hasAccess)
        {
            return Forbid();
        }

        if (expiry.TotalSeconds > 604800) // 7 days max
        {
            return BadRequest(ResponseData.Fail(code: "invalid_expiry", message: "Expiry cannot exceed 7 days."));
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var result = await objectGrain.GetSignedUrlAsync(bucketId, key, expiry, method, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }

    /// <summary>
    /// 初始化分片上传
    /// 用于大文件分片上传，先调用此接口获取 UploadId
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="request">请求参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分片上传信息</returns>
    [HttpPost("{bucketId:guid}/{*key}/multipart/initiate")]
    [Authorize(policy: "permission:storage.object.create")]
    public async Task<IActionResult> InitiateMultipartUpload(Guid bucketId, string key, [FromBody] InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        var result = await objectGrain.InitiateMultipartUploadAsync(bucketId, key, request.ContentType, request.Metadata, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }

    /// <summary>
    /// 上传分片
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="uploadId">分片上传 ID</param>
    /// <param name="file">分片文件内容</param>
    /// <param name="partNumber">分片编号 (从 1 开始)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分片上传结果</returns>
    [HttpPost("{bucketId:guid}/{*key}/multipart/{uploadId}/part")]
    [Authorize(policy: "permission:storage.object.create")]
    public async Task<IActionResult> UploadPart(Guid bucketId, string key, string uploadId, [FromForm] IFormFile file, [FromForm] int partNumber, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_file", message: "File is required."));
        }

        await using var stream = file.OpenReadStream();
        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        
        var result = await objectGrain.UploadPartAsync(bucketId, key, uploadId, partNumber, stream, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }

    /// <summary>
    /// 完成分片上传
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="uploadId">分片上传 ID</param>
    /// <param name="request">请求参数，包含所有分片信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    [HttpPost("{bucketId:guid}/{*key}/multipart/{uploadId}/complete")]
    [Authorize(policy: "permission:storage.object.create")]
    public async Task<IActionResult> CompleteMultipartUpload(Guid bucketId, string key, string uploadId, [FromBody] CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        
        try
        {
            var result = await objectGrain.CompleteMultipartUploadAsync(bucketId, key, uploadId, request.Parts, cancellationToken);
            return Ok(ResponseData.Success(data: result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseData.Fail(code: "multipart_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 取消分片上传
    /// </summary>
    /// <param name="bucketId">存储桶 GUID</param>
    /// <param name="key">对象键</param>
    /// <param name="uploadId">分片上传 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>取消结果</returns>
    [HttpDelete("{bucketId:guid}/{*key}/multipart/{uploadId}")]
    [Authorize(policy: "permission:storage.object.delete")]
    public async Task<IActionResult> AbortMultipartUpload(Guid bucketId, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasBucketAccessAsync(bucketId, "Write");
        if (!hasAccess)
        {
            return Forbid();
        }

        var objectGrain = _client.GetGrain<IObjectGrain>(0);
        await objectGrain.AbortMultipartUploadAsync(bucketId, key, uploadId, cancellationToken);
        return Ok(ResponseData.Success(data: true));
    }
}

/// <summary>
/// 初始化分片上传请求
/// </summary>
public class InitiateMultipartUploadRequest
{
    /// <summary>
    /// 对象的 Content-Type
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";
    
    /// <summary>
    /// 自定义元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// 完成分片上传请求
/// </summary>
public class CompleteMultipartUploadRequest
{
    /// <summary>
    /// 所有分片的 ETag 信息
    /// </summary>
    public List<PartETagDto> Parts { get; set; } = new();
}
