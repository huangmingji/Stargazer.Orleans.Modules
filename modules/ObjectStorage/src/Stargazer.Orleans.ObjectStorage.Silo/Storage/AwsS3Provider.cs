using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AwsS3Provider : IStorageProvider
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    public string ProviderName => "aws";

    public AwsS3Provider(AwsStorageSettings settings)
    {
        _bucketName = settings.BucketName;
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };
        _client = new AmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey, config);
    }

    public AwsS3Provider(AwsStorageSettings settings, IAmazonS3 client)
    {
        _bucketName = settings.BucketName;
        _client = client;
    }

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

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        await _client.DeleteObjectAsync(request, cancellationToken);
    }

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

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var response = await _client.ListBucketsAsync(cancellationToken);
        return response.Buckets.Any(b => b.BucketName == bucket);
    }

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
