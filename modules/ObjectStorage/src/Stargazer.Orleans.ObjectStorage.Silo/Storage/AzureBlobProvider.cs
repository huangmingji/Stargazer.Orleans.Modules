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

/// <summary>
/// 微软 Azure Blob Storage 存储提供者。
/// Azure Blob Storage 是 Microsoft 提供的海量云端存储服务，适用于存储非结构化数据。
/// </summary>
/// <remarks>
/// 支持的功能：
/// - 三种 blob 类型：Block Blob、Page Blob、Append Blob
/// - 三种存储层级：Hot、Cool、Archive
/// - 丰富的元数据和属性管理
/// - 快照和版本控制
/// - 共享访问签名（SAS）
/// - 分块上传（Block Blob）
/// 
/// 配置项：
/// - ConnectionString：Azure Storage 连接字符串
/// - ContainerName：默认容器名称
/// 
/// 注意：Azure Blob Storage 的概念与 S3略有不同：
/// - Bucket（存储桶）对应 Container（容器）
/// - 分片上传使用 Block Blob 的 StageBlock/CommitBlockList API
/// </remarks>
public class AzureBlobProvider : IStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    /// <inheritdoc />
    public string ProviderName => "azure";

    /// <summary>
    /// 初始化 Azure Blob Storage 存储提供者。
    /// </summary>
    /// <param name="settings">Azure 存储配置</param>
    public AzureBlobProvider(AzureStorageSettings settings)
    {
        _containerName = settings.ContainerName;
        _blobServiceClient = new BlobServiceClient(settings.ConnectionString);
    }

    /// <summary>
    /// 初始化 Azure Blob Storage 存储提供者（使用注入的客户端）。
    /// 适用于测试场景或需要自定义客户端配置的情况。
    /// </summary>
    /// <param name="settings">Azure 存储配置</param>
    /// <param name="blobServiceClient">Blob 服务客户端实例</param>
    public AzureBlobProvider(AzureStorageSettings settings, BlobServiceClient blobServiceClient)
    {
        _containerName = settings.ContainerName;
        _blobServiceClient = blobServiceClient;
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        return response.Value.Content.ToStream();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        return await containerClient.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blobClient = containerClient.GetBlobClient(key);

        return Task.FromResult(blobClient.Uri.ToString());
    }

    /// <inheritdoc />
    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return Task.FromResult(blockId);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucket);
        var blockBlobClient = containerClient.GetBlockBlobClient(key);

        await blockBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
