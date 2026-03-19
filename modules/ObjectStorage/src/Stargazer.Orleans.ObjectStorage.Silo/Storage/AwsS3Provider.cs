using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

/// <summary>
/// 亚马逊 AWS S3（Simple Storage Service）存储提供者。
/// AWS S3 是 Amazon 提供的对象存储服务，具有高可用性、高扩展性和高持久性。
/// </summary>
/// <remarks>
/// 支持的功能：
/// - 多种存储类别（Standard、IA、Glacier 等）
/// - 版本控制
/// - 生命周期管理
/// - 访问控制（ACL、IAM Policy）
/// - 签名 URL 生成
/// - 分片上传
/// - 跨区域复制（CRR）
/// 
/// 配置项：
/// - Region：AWS 区域（如 us-east-1）
/// - AccessKeyId/SecretAccessKey：AWS 访问凭证
/// - BucketName：默认存储桶名称
/// </remarks>
public class AwsS3Provider : IStorageProvider
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    /// <inheritdoc />
    public string ProviderName => "aws";

    /// <summary>
    /// 初始化 AWS S3 存储提供者。
    /// </summary>
    /// <param name="settings">AWS 存储配置</param>
    public AwsS3Provider(AwsStorageSettings settings)
    {
        _bucketName = settings.BucketName;
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };
        _client = new AmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey, config);
    }

    /// <summary>
    /// 初始化 AWS S3 存储提供者（使用注入的客户端）。
    /// 适用于测试场景或需要自定义客户端配置的情况。
    /// </summary>
    /// <param name="settings">AWS 存储配置</param>
    /// <param name="client">S3 客户端实例</param>
    public AwsS3Provider(AwsStorageSettings settings, IAmazonS3 client)
    {
        _bucketName = settings.BucketName;
        _client = client;
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        var response = await _client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    /// <inheritdoc />
    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = content,
            ContentType = metadata.ContentType
        };

        await _client.PutObjectAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        await _client.DeleteObjectAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucket,
                Key = key
            };
            await _client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = bucket,
            Key = key
        };

        var response = await _client.GetObjectMetadataAsync(request, cancellationToken);

        return new StorageMetadata
        {
            ContentLength = response.ContentLength,
            ContentType = response.Headers.ContentType,
            ETag = response.ETag,
            LastModified = response.LastModified,
            CacheControl = response.Headers.CacheControl ?? string.Empty,
            ContentDisposition = response.Headers.ContentDisposition ?? string.Empty,
            ContentEncoding = response.Headers.ContentEncoding ?? string.Empty
        };
    }

    /// <inheritdoc />
    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefix ?? string.Empty
        };

        var response = await _client.ListObjectsV2Async(request, cancellationToken);

        return response.S3Objects.Select(obj => new ObjectInfo
        {
            Key = obj.Key,
            Size = obj.Size ?? 0,
            LastModified = obj.LastModified,
            ETag = obj.ETag,
            StorageClass = obj.StorageClass.ToString()
        }).ToList();
    }

    /// <inheritdoc />
    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new PutBucketRequest
            {
                BucketName = bucket
            };
            await _client.PutBucketAsync(request, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new DeleteBucketRequest
            {
                BucketName = bucket
            };
            await _client.DeleteBucketAsync(request, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var response = await _client.ListBucketsAsync(cancellationToken);
        return response.Buckets.Any(b => b.BucketName == bucket);
    }

    /// <inheritdoc />
    public async Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = method == HttpMethod.Get ? HttpVerb.GET : HttpVerb.PUT
        };

        return await _client.GetPreSignedURLAsync(request);
    }

    /// <inheritdoc />
    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = bucket,
            Key = key,
            ContentType = metadata.ContentType
        };

        var response = await _client.InitiateMultipartUploadAsync(request, cancellationToken);
        return response.UploadId;
    }

    /// <inheritdoc />
    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var request = new UploadPartRequest
        {
            BucketName = bucket,
            Key = key,
            UploadId = uploadId,
            PartNumber = partNumber,
            InputStream = content
        };

        var response = await _client.UploadPartAsync(request, cancellationToken);
        return response.ETag;
    }

    /// <inheritdoc />
    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = bucket,
            Key = key,
            UploadId = uploadId,
            PartETags = parts.Select(p => new Amazon.S3.Model.PartETag { ETag = p.ETag, PartNumber = p.PartNumber }).ToList()
        };

        await _client.CompleteMultipartUploadAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = bucket,
            Key = key,
            UploadId = uploadId
        };

        await _client.AbortMultipartUploadAsync(request, cancellationToken);
    }
}
