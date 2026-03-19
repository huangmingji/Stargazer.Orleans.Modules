using System.Net;
using System.IO;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

/// <summary>
/// MinIO 存储提供者。
/// MinIO 是一款高性能、分布式的对象存储系统，兼容 Amazon S3 API。
/// 适用于私有云、边缘计算、Kubernetes 环境等场景。
/// </summary>
/// <remarks>
/// 支持的功能：
/// - S3 兼容的 API 接口
/// - 高性能对象存储
/// - 分布式部署（纠删码模式）
/// - 签名 URL 生成
/// - 存储桶和对象管理
/// 
/// 配置项：
/// - Endpoint：MinIO 服务地址（如 localhost:9000）
/// - AccessKey/SecretKey：访问凭证
/// - UseSSL：是否使用 HTTPS 连接
/// - BucketName：默认存储桶名称
/// 
/// 注意：
/// - MinIO SDK 暂不支持原生的 S3 分片上传 API
/// - 分片上传通过创建临时对象模拟实现，然后将分片合并为最终对象
/// - 生产环境中建议使用原生 S3 兼容服务（如 AWS S3、MinIO Server）
///   以获得完整的分片上传支持和更好的性能
/// </remarks>
public class MinioProvider : IStorageProvider
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    /// <inheritdoc />
    public string ProviderName => "minio";

    /// <summary>
    /// 初始化 MinIO 存储提供者。
    /// </summary>
    /// <param name="settings">MinIO 存储配置</param>
    public MinioProvider(MinioStorageSettings settings)
    {
        _bucketName = settings.BucketName;

        var builder = new MinioClient().WithEndpoint(settings.Endpoint);

        if (settings.UseSSL)
        {
            builder.WithSSL();
        }

        if (!string.IsNullOrEmpty(settings.AccessKey) && !string.IsNullOrEmpty(settings.SecretKey))
        {
            builder.WithCredentials(settings.AccessKey, settings.SecretKey);
        }

        _minioClient = builder.Build();
    }

    /// <summary>
    /// 初始化 MinIO 存储提供者（使用注入的客户端）。
    /// 适用于测试场景或需要自定义客户端配置的情况。
    /// </summary>
    /// <param name="settings">MinIO 存储配置</param>
    /// <param name="minioClient">MinIO 客户端实例</param>
    public MinioProvider(MinioStorageSettings settings, IMinioClient minioClient)
    {
        _bucketName = settings.BucketName;
        _minioClient = minioClient;
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        
        var args = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
            });

        await _minioClient.GetObjectAsync(args, cancellationToken);
        
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(metadata.ContentType ?? "application/octet-stream");

        if (!string.IsNullOrEmpty(metadata.ContentType))
        {
            args.WithContentType(metadata.ContentType);
        }

        await _minioClient.PutObjectAsync(args, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(key);

        await _minioClient.RemoveObjectAsync(args, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(bucket)
                .WithObject(key);

            await _minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var args = new StatObjectArgs()
            .WithBucket(bucket)
            .WithObject(key);

        var stat = await _minioClient.StatObjectAsync(args, cancellationToken);

        return new StorageMetadata
        {
            ContentLength = stat.Size,
            ContentType = stat.ContentType,
            ETag = stat.ETag,
            LastModified = stat.LastModified,
            CacheControl = stat.MetaData?.GetValueOrDefault("cache-control", ""),
            ContentDisposition = stat.MetaData?.GetValueOrDefault("content-disposition", ""),
            ContentEncoding = stat.MetaData?.GetValueOrDefault("content-encoding", ""),
            Metadata = stat.MetaData ?? new Dictionary<string, string>()
        };
    }

    /// <inheritdoc />
    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var objectList = new List<ObjectInfo>();

        var args = new ListObjectsArgs()
            .WithBucket(bucket)
            .WithRecursive(true);

        if (!string.IsNullOrEmpty(prefix))
        {
            args.WithPrefix(prefix);
        }

        var observable = _minioClient.ListObjectsEnumAsync(args, cancellationToken);

        await foreach (var item in observable)
        {
            objectList.Add(new ObjectInfo
            {
                Key = item.Key,
                Size = (long)item.Size,
                LastModified = item.LastModifiedDateTime,
                ETag = item.ETag,
                StorageClass = "STANDARD"
            });
        }

        return objectList;
    }

    /// <inheritdoc />
    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucket, cancellationToken))
        {
            var args = new MakeBucketArgs()
                .WithBucket(bucket);

            await _minioClient.MakeBucketAsync(args, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (await BucketExistsAsync(bucket, cancellationToken))
        {
            var args = new RemoveBucketArgs()
                .WithBucket(bucket);

            await _minioClient.RemoveBucketAsync(args, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var args = new BucketExistsArgs()
            .WithBucket(bucket);

        return await _minioClient.BucketExistsAsync(args, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var expirySeconds = (int)expiry.TotalSeconds;

        if (method == HttpMethod.Get)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithExpiry(expirySeconds);

            return await _minioClient.PresignedGetObjectAsync(args);
        }
        else if (method == HttpMethod.Put)
        {
            var args = new PresignedPutObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithExpiry(expirySeconds);

            return await _minioClient.PresignedPutObjectAsync(args);
        }
        else
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithExpiry(expirySeconds);

            return await _minioClient.PresignedGetObjectAsync(args);
        }
    }

    /// <inheritdoc />
    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"{Guid.NewGuid()}");
    }

    /// <inheritdoc />
    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject($"{key}.{uploadId}.part{partNumber}")
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length)
            .WithContentType("application/octet-stream");

        await _minioClient.PutObjectAsync(args, cancellationToken);

        return $"\"{Guid.NewGuid()}\"";
    }

    /// <inheritdoc />
    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        
        foreach (var part in parts.OrderBy(p => p.PartNumber))
        {
            var partArgs = new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject($"{key}.{uploadId}.part{part.PartNumber}")
                .WithCallbackStream(s => s.CopyTo(memoryStream));
            
            await _minioClient.GetObjectAsync(partArgs, cancellationToken);
        }

        memoryStream.Position = 0;
        
        var putArgs = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length)
            .WithContentType("application/octet-stream");

        await _minioClient.PutObjectAsync(putArgs, cancellationToken);

        foreach (var part in parts)
        {
            var deleteArgs = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject($"{key}.{uploadId}.part{part.PartNumber}");
            
            await _minioClient.RemoveObjectAsync(deleteArgs, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var args = new RemoveIncompleteUploadArgs()
            .WithBucket(bucket)
            .WithObject(key);

        await _minioClient.RemoveIncompleteUploadAsync(args, cancellationToken);
    }
}
