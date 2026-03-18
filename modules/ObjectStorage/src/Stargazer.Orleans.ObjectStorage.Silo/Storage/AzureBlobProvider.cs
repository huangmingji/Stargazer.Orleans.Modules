using Azure.Storage.Blobs;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AzureBlobProvider : IStorageProvider
{
    private readonly BlobServiceClient _client;
    private readonly string _containerName;

    public string ProviderName => "azure";

    public AzureBlobProvider(AzureStorageSettings settings)
    {
        _client = new BlobServiceClient(settings.ConnectionString);
        _containerName = settings.ContainerName;
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blob = container.GetBlobClient(key);
        var response = await blob.DownloadToAsync(cancellationToken);
        return response;
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blob = container.GetBlobClient(key);
        await blob.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = metadata.ContentType
            }
        }, cancellationToken);
    }

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blob = container.GetBlobClient(key);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blob = container.GetBlobClient(key);
        return await blob.ExistsAsync(cancellationToken);
    }

    public async Task<ObjectMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blob = container.GetBlobClient(key);
        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);
        return new ObjectMetadata
        {
            ContentLength = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType,
            ETag = properties.Value.ETag.ToString(),
            LastModified = properties.Value.LastModified.DateTime
        };
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blobs = container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken);
        
        var result = new List<ObjectInfo>();
        await foreach (var blobItem in blobs)
        {
            result.Add(new ObjectInfo
            {
                Key = blobItem.Name,
                Size = blobItem.Properties.ContentLength ?? 0,
                LastModified = blobItem.Properties.LastModified?.DateTime,
                ETag = blobItem.Properties.ETag.ToString()
            });
        }
        return result;
    }

    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        await container.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        return await container.ExistsAsync(cancellationToken);
    }

    public async Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(bucket);
        var blob = container.GetBlobClient(key);
        var url = await blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow + expiry);
        return url.ToString();
    }

    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Azure Blob Storage uses different multipart upload mechanism");
    }

    public Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Azure Blob Storage uses different multipart upload mechanism");
    }

    public Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Azure Blob Storage uses different multipart upload mechanism");
    }

    public Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Azure Blob Storage uses different multipart upload mechanism");
    }
}
