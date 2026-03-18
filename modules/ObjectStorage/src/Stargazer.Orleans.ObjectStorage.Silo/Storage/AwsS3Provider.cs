using Amazon.S3;
using Amazon.S3.Model;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AwsS3Provider : IStorageProvider
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    public string ProviderName => "aws";

    public AwsS3Provider(AwsStorageSettings settings)
    {
        var config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.Region) };
        _client = new AmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey, config);
        _bucketName = settings.BucketName;
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetObjectAsync(bucket, key, cancellationToken);
        return response.ResponseStream;
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = content,
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
        var response = await _client.GetObjectMetadataAsync(bucket, key, cancellationToken);
        return new ObjectMetadata
        {
            ContentLength = response.ContentLength,
            ContentType = response.ContentType,
            ETag = response.ETag,
            LastModified = response.LastModified
        };
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsV2Request { BucketName = bucket, Prefix = prefix };
        var response = await _client.ListObjectsV2Async(request, cancellationToken);
        
        return response.S3Objects.Select(o => new ObjectInfo
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
        await _client.PutBucketAsync(bucket, cancellationToken);
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
        var url = _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow + expiry,
            HttpMethod = method == HttpMethod.Get ? HttpVerb.GET : HttpVerb.PUT
        });
        return url;
    }

    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitiateMultipartUploadRequest { BucketName = bucket, Key = key, ContentType = metadata.ContentType };
        var response = await _client.InitiateMultipartUploadAsync(request, cancellationToken);
        return response.UploadId;
    }

    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var request = new UploadPartRequest { BucketName = bucket, Key = key, UploadId = uploadId, PartNumber = partNumber, InputStream = content };
        var response = await _client.UploadPartAsync(request, cancellationToken);
        return response.ETag;
    }

    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest { BucketName = bucket, Key = key, UploadId = uploadId };
        request.AddPartETags(parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList());
        await _client.CompleteMultipartUploadAsync(request, cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        await _client.AbortMultipartUploadAsync(bucket, key, uploadId, cancellationToken);
    }
}
