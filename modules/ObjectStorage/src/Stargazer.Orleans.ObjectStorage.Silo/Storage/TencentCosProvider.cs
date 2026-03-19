using System.Net;
using System.IO;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.CosException;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;
using COSXML.Model.Tag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

/// <summary>
/// 腾讯云 COS（Cloud Object Storage）存储提供者。
/// 腾讯云 COS 是腾讯云提供的对象存储服务，具有高可用、高可靠、强安全的特点。
/// </summary>
/// <remarks>
/// 支持的功能：
/// - 多种存储类型：标准存储、低频存储、归档存储、深度归档存储
/// - 强大的数据处理能力（图片处理、音视频转码等）
/// - 细粒度的权限控制（ACL、IAM、CAM）
/// - 签名 URL 生成
/// - 分片上传
/// - 跨域访问控制（CORS）
/// 
/// 配置项：
/// - Region：COS 地域（如 ap-guangzhou）
/// - SecretId/SecretKey：云 API 密钥
/// - BucketName：COS 存储桶名称（格式：{BucketName}-{AppId}）
/// 
/// 注意：
/// - GetObject 和 PutObject 操作需要临时文件作为中转
/// - 分片上传使用 UploadPart API，支持断点续传
/// </remarks>
public class TencentCosProvider : IStorageProvider
{
    private readonly CosXml _cosXml;
    private readonly string _bucketName;

    /// <inheritdoc />
    public string ProviderName => "tencent";

    /// <summary>
    /// 初始化腾讯云 COS 存储提供者。
    /// </summary>
    /// <param name="settings">腾讯云存储配置</param>
    public TencentCosProvider(TencentStorageSettings settings)
    {
        _bucketName = settings.BucketName;

        var config = new CosXmlConfig.Builder()
            .IsHttps(true)
            .SetRegion(settings.Region)
            .Build();

        var qCloudCredentialProvider = new DefaultQCloudCredentialProvider(
            settings.SecretId,
            settings.SecretKey,
            600);

        _cosXml = new CosXmlServer(config, qCloudCredentialProvider);
    }

    /// <summary>
    /// 初始化腾讯云 COS 存储提供者（使用注入的客户端）。
    /// 适用于测试场景或需要自定义客户端配置的情况。
    /// </summary>
    /// <param name="settings">腾讯云存储配置</param>
    /// <param name="cosXml">COS XML 客户端实例</param>
    public TencentCosProvider(TencentStorageSettings settings, CosXml cosXml)
    {
        _bucketName = settings.BucketName;
        _cosXml = cosXml;
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
        
        try
        {
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);
            
            var bytes = await File.ReadAllBytesAsync(tempFile, cancellationToken);
            return new MemoryStream(bytes);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <inheritdoc />
    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            await using (var fileStream = File.Create(tempFile))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            var request = new PutObjectRequest(bucket, key, tempFile);
            
            await Task.Run(() => _cosXml.PutObject(request), cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest(bucket, key);
        await Task.Run(() => _cosXml.DeleteObject(request), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var tempDir = Path.GetTempPath();
            var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);
            
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            return true;
        }
        catch (CosClientException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
        
        try
        {
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);

            var fileInfo = new FileInfo(tempFile);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            return new StorageMetadata
            {
                ContentLength = fileInfo.Exists ? fileInfo.Length : 0,
                ContentType = "",
                ETag = "",
                LastModified = DateTime.MinValue,
                CacheControl = "",
                ContentDisposition = "",
                ContentEncoding = "",
                Metadata = new Dictionary<string, string>()
            };
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <inheritdoc />
    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new GetBucketRequest(bucket);
        if (!string.IsNullOrEmpty(prefix))
        {
            request.SetPrefix(prefix);
        }

        var result = await Task.Run(() => _cosXml.GetBucket(request), cancellationToken);

        var objectList = new List<ObjectInfo>();
        if (result.listBucket?.contentsList != null)
        {
            foreach (var obj in result.listBucket.contentsList)
            {
                objectList.Add(new ObjectInfo
                {
                    Key = obj.key,
                    Size = obj.size,
                    LastModified = string.IsNullOrEmpty(obj.lastModified) ? null : DateTime.Parse(obj.lastModified),
                    ETag = obj.eTag,
                    StorageClass = obj.storageClass
                });
            }
        }

        return objectList;
    }

    /// <inheritdoc />
    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new PutBucketRequest(bucket);
            await Task.Run(() => _cosXml.PutBucket(request), cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new DeleteBucketRequest(bucket);
            await Task.Run(() => _cosXml.DeleteBucket(request), cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetBucketRequest(bucket);
            await Task.Run(() => _cosXml.GetBucket(request), cancellationToken);
            return true;
        }
        catch (CosClientException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var preSignatureStruct = new PreSignatureStruct
        {
            bucket = bucket,
            key = key,
            httpMethod = method.Method,
            signDurationSecond = (int)expiry.TotalSeconds
        };
        
        var url = _cosXml.GenerateSignURL(preSignatureStruct);
        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitMultipartUploadRequest(bucket, key);
        
        var result = await Task.Run(() => _cosXml.InitMultipartUpload(request), cancellationToken);
        return result.initMultipartUpload.uploadId;
    }

    /// <inheritdoc />
    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);
        var data = memoryStream.ToArray();

        var request = new UploadPartRequest(bucket, key, partNumber, uploadId, data);

        var result = await Task.Run(() => _cosXml.UploadPart(request), cancellationToken);
        return result.eTag;
    }

    /// <inheritdoc />
    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest(bucket, key, uploadId);

        foreach (var part in parts)
        {
            request.SetPartNumberAndETag(part.PartNumber, part.ETag);
        }

        await Task.Run(() => _cosXml.CompleteMultiUpload(request), cancellationToken);
    }

    /// <inheritdoc />
    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest(bucket, key, uploadId);
        await Task.Run(() => _cosXml.AbortMultiUpload(request), cancellationToken);
    }
}
