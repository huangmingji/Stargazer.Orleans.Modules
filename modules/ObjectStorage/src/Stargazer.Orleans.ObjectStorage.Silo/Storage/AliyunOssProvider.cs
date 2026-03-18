using Aliyun.OSS;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AliyunOssProvider : IStorageProvider
{
    private readonly OssClient _client;
    private readonly string _bucketName;

    public string ProviderName => "aliyun";

    public AliyunOssProvider(AliyunStorageSettings settings)
    {
        _client = new OssClient(settings.Endpoint, settings.AccessKeyId, settings.AccessKeySecret);
        _bucketName = settings.BucketName;
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var obj = await _client.GetObjectAsync(bucket, key, cancellationToken);
        return obj.Content;
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest(bucket, key, content)
        {
            ContentType = metadata.ContentType
        };
        foreach (var meta in metadata.Metadata)
        {
            request.Metadata.Add(meta.Key, meta.Value);
        }
        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(bucket, key, cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        return await _client.DoesObjectExistAsync(bucket, key, cancellationToken);
    }

    public async Task<ObjectMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var result = await _client.GetObjectMetadataAsync(bucket, key, cancellationToken);
        return new ObjectMetadata
        {
            ContentLength = result.ContentLength,
            ContentType = result.ContentType,
            ETag = result.ETag,
            LastModified = result.LastModified,
            Metadata = result.Metadata
        };
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsRequest(bucket) { Prefix = prefix };
        var result = await _client.ListObjectsAsync(request, cancellationToken);
        
        return result.ObjectSummaries.Select(o => new ObjectInfo
        {
            Key = o.Key,
            Size = o.Size,
            LastModified = o.LastModified,
            ETag = o.ETag,
            StorageClass = o.StorageClass
        }).ToList();
    }

    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        await _client.CreateBucketAsync(bucket, cancellationToken);
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        await _client.DeleteBucketAsync(bucket, cancellationToken);
    }

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        return await _client.DoesBucketExistAsync(bucket, cancellationToken);
    }

    public async Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var signUrl = _client.GeneratePresignedUrl(bucket, key, DateTime.UtcNow + expiry);
        return signUrl.ToString();
    }

    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitiateMultipartUploadRequest(bucket, key)
        {
            ContentType = metadata.ContentType
        };
        var result = await _client.InitiateMultipartUploadAsync(request, cancellationToken);
        return result.UploadId;
    }

    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var request = new UploadPartRequest(bucket, key, uploadId, partNumber, content);
        var result = await _client.UploadPartAsync(request, cancellationToken);
        return result.ETag;
    }

    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest(bucket, key, uploadId);
        request.PartETags = parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList();
        await _client.CompleteMultipartUploadAsync(request, cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest(bucket, key, uploadId);
        await _client.AbortMultipartUploadAsync(request, cancellationToken);
    }
}
