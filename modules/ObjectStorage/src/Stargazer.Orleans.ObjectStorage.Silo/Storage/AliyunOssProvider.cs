using System.Net;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AliyunOssProvider: IStorageProvider
{
    private readonly OssClient _client;
    private readonly string _bucketName;

    public string ProviderName => "aliyun";

    public AliyunOssProvider(AliyunStorageSettings settings)
    {
        _bucketName = settings.BucketName;

        var conf = new ClientConfiguration
        {
            SignatureVersion = SignatureVersion.V4
        };

        _client = new OssClient(settings.Endpoint, settings.AccessKeyId, settings.AccessKeySecret, conf);
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest(bucket, key);
        var result = await Task.Run(() => _client.GetObject(request), cancellationToken);
        return result.Content;
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest(bucket, key, content);

        await Task.Run(() => _client.PutObject(request), cancellationToken);
    }

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => _client.DeleteObject(bucket, key), cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _client.DoesObjectExist(bucket, key), cancellationToken);
    }

    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var result = await Task.Run(() => _client.GetObjectMetadata(bucket, key), cancellationToken);

        return new StorageMetadata
        {
            ContentLength = result.ContentLength,
            ContentType = result.ContentType,
            ETag = result.ETag,
            LastModified = result.LastModified,
            CacheControl = result.CacheControl ?? string.Empty,
            ContentDisposition = result.ContentDisposition ?? string.Empty,
            ContentEncoding = result.ContentEncoding ?? string.Empty,
            Metadata = result.UserMetadata?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>()
        };
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsRequest(bucket)
        {
            Prefix = prefix ?? string.Empty
        };

        var result = await Task.Run(() => _client.ListObjects(request), cancellationToken);

        return result.ObjectSummaries.Select(obj => new ObjectInfo
        {
            Key = obj.Key,
            Size = obj.Size,
            LastModified = obj.LastModified,
            ETag = obj.ETag,
            StorageClass = obj.StorageClass
        }).ToList();
    }

    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new CreateBucketRequest(bucket);
            await Task.Run(() => _client.CreateBucket(request), cancellationToken);
        }
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (await BucketExistsAsync(bucket, cancellationToken))
        {
            await Task.Run(() => _client.DeleteBucket(bucket), cancellationToken);
        }
    }

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _client.DoesBucketExist(bucket), cancellationToken);
    }

    public async Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.Add(expiry);

        var signedUrl = await Task.Run(() =>
            _client.GeneratePresignedUri(bucket, key, expires), cancellationToken);

        return signedUrl.ToString();
    }

    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitiateMultipartUploadRequest(bucket, key);

        var result = await Task.Run(() => _client.InitiateMultipartUpload(request), cancellationToken);
        return result.UploadId;
    }

    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var uploadPartRequest = new UploadPartRequest(bucket, key, uploadId)
        {
            InputStream = content,
            PartSize = content.Length,
            PartNumber = partNumber
        };

        var result = await Task.Run(() => _client.UploadPart(uploadPartRequest), cancellationToken);
        return result.ETag;
    }

    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest(bucket, key, uploadId);
        foreach (var part in parts)
        {
            request.PartETags.Add(new Aliyun.OSS.PartETag(part.PartNumber, part.ETag));
        }

        await Task.Run(() => _client.CompleteMultipartUpload(request), cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest(bucket, key, uploadId);
        await Task.Run(() => _client.AbortMultipartUpload(request), cancellationToken);
    }
}
