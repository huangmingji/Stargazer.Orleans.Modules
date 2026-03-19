using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class AzureBlobProvider : IStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public string ProviderName => "azure";

    public AzureBlobProvider(AzureStorageSettings settings)
    {
        _containerName = settings.ContainerName;
        _blobServiceClient = new BlobServiceClient(settings.ConnectionString);
    }

    public AzureBlobProvider(AzureStorageSettings settings, BlobServiceClient blobServiceClient)
    {
        _containerName = settings.ContainerName;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        return response.Value.Content.ToStream();
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = metadata.ContentType
            },
            Metadata = metadata.Metadata
        };

        await blobClient.UploadAsync(content, options, cancellationToken);
    }

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new StorageMetadata
        {
            ContentLength = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType,
            ETag = properties.Value.ETag.ToString(),
            LastModified = properties.Value.LastModified.LocalDateTime,
            CacheControl = properties.Value.CacheControl ?? string.Empty,
            ContentDisposition = properties.Value.ContentDisposition ?? string.Empty,
            ContentEncoding = properties.Value.ContentEncoding ?? string.Empty,
            Metadata = new Dictionary<string, string>(properties.Value.Metadata)
        };
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var results = new List<ObjectInfo>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix ?? string.Empty, cancellationToken))
        {
            results.Add(new ObjectInfo
            {
                Key = blobItem.Name,
                Size = blobItem.Properties.ContentLength ?? 0,
                LastModified = blobItem.Properties.LastModified?.LocalDateTime ?? DateTime.MinValue,
                ETag = blobItem.Properties.ETag.ToString(),
                StorageClass = blobItem.Properties.BlobType.ToString()
            });
        }

        return results;
    }

    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        return await containerClient.ExistsAsync(cancellationToken);
    }

    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        return Task.FromResult(blobClient.Uri.ToString());
    }

    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return Task.FromResult(blockId);
    }

    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blockBlobClient = containerClient.GetBlockBlobClient(key);

        var blockId = Convert.ToBase64String(BitConverter.GetBytes(partNumber));

        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        await blockBlobClient.StageBlockAsync(blockId, memoryStream, cancellationToken: cancellationToken);

        return blockId;
    }

    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blockBlobClient = containerClient.GetBlockBlobClient(key);

        var committedBlocks = new List<string>();

        foreach (var part in parts.OrderBy(p => p.PartNumber))
        {
            var blockId = Convert.ToBase64String(BitConverter.GetBytes(part.PartNumber));
            committedBlocks.Add(blockId);
        }

        await blockBlobClient.CommitBlockListAsync(committedBlocks, cancellationToken: cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blockBlobClient = containerClient.GetBlockBlobClient(key);

        await blockBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
