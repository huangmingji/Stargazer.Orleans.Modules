using System.Net;
using System.Security.Cryptography;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

/// <summary>
/// 本地文件系统存储提供者，将对象存储在本地磁盘目录中。
/// 适用于开发测试环境或单机部署场景，生产环境建议使用云存储服务。
/// </summary>
/// <remarks>
/// 存储结构：{BasePath}/{bucket}/{key}
/// 每个对象对应一个文件，元数据通过文件系统的文件属性存储。
/// ETag 通过 MD5 哈希计算生成。
/// </remarks>
public class LocalStorageProvider : IStorageProvider
{
    private readonly string _basePath;

    /// <inheritdoc />
    public string ProviderName => "local";

    /// <summary>
    /// 初始化本地存储提供者。
    /// </summary>
    /// <param name="settings">本地存储配置</param>
    public LocalStorageProvider(LocalStorageSettings settings)
    {
        _basePath = settings.BasePath;
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Object {key} not found in bucket {bucket}");
        }
        return await Task.FromResult<Stream>(File.OpenRead(path));
    }

    /// <inheritdoc />
    public async Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        if (File.Exists(path))
        {
            await Task.Run(() => File.Delete(path), cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        return Task.FromResult(File.Exists(path));
    }

    /// <inheritdoc />
    public Task<ObjectMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Object {key} not found in bucket {bucket}");
        }

        var fileInfo = new FileInfo(path);
        return Task.FromResult(new ObjectMetadata
        {
            ContentLength = fileInfo.Length,
            LastModified = fileInfo.LastWriteTimeUtc,
            ETag = GetETag(path),
            ContentType = "application/octet-stream"
        });
    }

    /// <inheritdoc />
    public Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var bucketPath = Path.Combine(_basePath, bucket);
        if (!Directory.Exists(bucketPath))
        {
            return Task.FromResult(new List<ObjectInfo>());
        }

        var searchPattern = string.IsNullOrEmpty(prefix) ? "*" : prefix + "*";
        var files = Directory.GetFiles(bucketPath, searchPattern, SearchOption.AllDirectories);
        
        var objects = files.Select(f => new FileInfo(f)).Select(fi => new ObjectInfo
        {
            Key = Path.GetRelativePath(bucketPath, fi.FullName).Replace("\\", "/"),
            Size = fi.Length,
            LastModified = fi.LastWriteTimeUtc,
            ETag = GetETag(fi.FullName)
        }).ToList();

        return Task.FromResult(objects);
    }

    /// <inheritdoc />
    public Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var bucketPath = Path.Combine(_basePath, bucket);
        if (!Directory.Exists(bucketPath))
        {
            Directory.CreateDirectory(bucketPath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var bucketPath = Path.Combine(_basePath, bucket);
        if (Directory.Exists(bucketPath))
        {
            Directory.Delete(bucketPath, true);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        var bucketPath = Path.Combine(_basePath, bucket);
        return Task.FromResult(Directory.Exists(bucketPath));
    }

    /// <inheritdoc />
    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(bucket, key);
        return Task.FromResult($"file://{path}");
    }

    /// <inheritdoc />
    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default)
    {
        var uploadId = Guid.NewGuid().ToString();
        return Task.FromResult(uploadId);
    }

    /// <inheritdoc />
    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        var partPath = GetPartPath(bucket, key, uploadId, partNumber);
        using var fileStream = File.Create(partPath);
        await content.CopyToAsync(fileStream, cancellationToken);
        return GetETag(partPath);
    }

    /// <inheritdoc />
    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default)
    {
        var finalPath = GetFilePath(bucket, key);
        var directory = Path.GetDirectoryName(finalPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var finalStream = File.Create(finalPath);
        for (int i = 1; i <= parts.Count; i++)
        {
            var partPath = GetPartPath(bucket, key, uploadId, i);
            if (File.Exists(partPath))
            {
                await using var partStream = File.OpenRead(partPath);
                await partStream.CopyToAsync(finalStream, cancellationToken);
            }
        }
        
        CleanupParts(bucket, key, uploadId);
    }

    /// <inheritdoc />
    public Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        CleanupParts(bucket, key, uploadId);
        return Task.CompletedTask;
    }

    private string GetFilePath(string bucket, string key)
    {
        return Path.Combine(_basePath, bucket, key);
    }

    private string GetPartPath(string bucket, string key, string uploadId, int partNumber)
    {
        return Path.Combine(_basePath, bucket, $"{key}.part{uploadId}.{partNumber}");
    }

    private void CleanupParts(string bucket, string key, string uploadId)
    {
        var searchPattern = $"*.part{uploadId}.*";
        var directory = Path.Combine(_basePath, bucket);
        if (Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory, searchPattern))
            {
                File.Delete(file);
            }
        }
    }

    private string GetETag(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
