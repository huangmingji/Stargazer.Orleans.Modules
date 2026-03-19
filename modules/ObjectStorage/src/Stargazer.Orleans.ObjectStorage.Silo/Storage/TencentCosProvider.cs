using System.Net;
using System.IO;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.CosException;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

using StorageMetadata = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.ObjectMetadata;
using StoragePartETag = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.PartETag;
using COSXML.Model.Tag;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class TencentCosProvider : IStorageProvider
{
    private readonly CosXml _cosXml;
    private readonly string _bucketName;

    public string ProviderName => "tencent";

    public TencentCosProvider(TencentStorageSettings settings)
    {
        _bucketName = settings.BucketName;

        var config = new CosXmlConfig.Builder()
            .IsHttps(true)
            .SetRegion(settings.Region)
            .Build();

        var qCloudCredentialProvider = new DefaultQCloudCredentialProvider(
            settings.SecretId,
            settings.SecretKey,
            600);

        _cosXml = new CosXmlServer(config, qCloudCredentialProvider);
    }

    public TencentCosProvider(TencentStorageSettings settings, CosXml cosXml)
    {
        _bucketName = settings.BucketName;
        _cosXml = cosXml;
    }

    public async Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
        
        try
        {
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);
            
            var bytes = await File.ReadAllBytesAsync(tempFile, cancellationToken);
            return new MemoryStream(bytes);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    public async Task PutObjectAsync(string bucket, string key, Stream content, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            await using (var fileStream = File.Create(tempFile))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            var request = new PutObjectRequest(bucket, key, tempFile);
            
            await Task.Run(() => _cosXml.PutObject(request), cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest(bucket, key);
        await Task.Run(() => _cosXml.DeleteObject(request), cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var tempDir = Path.GetTempPath();
            var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);
            
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            return true;
        }
        catch (CosClientException)
        {
            return false;
        }
    }

    public async Task<StorageMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
        
        try
        {
            var request = new GetObjectRequest(bucket, key, tempDir, tempFile);
            await Task.Run(() => _cosXml.GetObject(request), cancellationToken);

            var fileInfo = new FileInfo(tempFile);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            return new StorageMetadata
            {
                ContentLength = fileInfo.Exists ? fileInfo.Length : 0,
                ContentType = "",
                ETag = "",
                LastModified = DateTime.MinValue,
                CacheControl = "",
                ContentDisposition = "",
                ContentEncoding = "",
                Metadata = new Dictionary<string, string>()
            };
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    public async Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
    {
        var request = new GetBucketRequest(bucket);
        if (!string.IsNullOrEmpty(prefix))
        {
            request.SetPrefix(prefix);
        }

        var result = await Task.Run(() => _cosXml.GetBucket(request), cancellationToken);

        var objectList = new List<ObjectInfo>();
        if (result.listBucket?.contentsList != null)
        {
            foreach (var obj in result.listBucket.contentsList)
            {
                objectList.Add(new ObjectInfo
                {
                    Key = obj.key,
                    Size = obj.size,
                    LastModified = string.IsNullOrEmpty(obj.lastModified) ? null : DateTime.Parse(obj.lastModified),
                    ETag = obj.eTag,
                    StorageClass = obj.storageClass
                });
            }
        }

        return objectList;
    }

    public async Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new PutBucketRequest(bucket);
            await Task.Run(() => _cosXml.PutBucket(request), cancellationToken);
        }
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
    {
        if (await BucketExistsAsync(bucket, cancellationToken))
        {
            var request = new DeleteBucketRequest(bucket);
            await Task.Run(() => _cosXml.DeleteBucket(request), cancellationToken);
        }
    }

    public async Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetBucketRequest(bucket);
            await Task.Run(() => _cosXml.GetBucket(request), cancellationToken);
            return true;
        }
        catch (CosClientException)
        {
            return false;
        }
    }

    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var preSignatureStruct = new PreSignatureStruct
        {
            bucket = bucket,
            key = key,
            httpMethod = method.Method,
            signDurationSecond = (int)expiry.TotalSeconds
        };
        
        var url = _cosXml.GenerateSignURL(preSignatureStruct);
        return Task.FromResult(url);
    }

    public async Task<string> InitiateMultipartUploadAsync(string bucket, string key, StorageMetadata metadata, CancellationToken cancellationToken = default)
    {
        var request = new InitMultipartUploadRequest(bucket, key);
        
        var result = await Task.Run(() => _cosXml.InitMultipartUpload(request), cancellationToken);
        return result.initMultipartUpload.uploadId;
    }

    public async Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);
        var data = memoryStream.ToArray();

        var request = new UploadPartRequest(bucket, key, partNumber, uploadId, data);

        var result = await Task.Run(() => _cosXml.UploadPart(request), cancellationToken);
        return result.eTag;
    }

    public async Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<StoragePartETag> parts, CancellationToken cancellationToken = default)
    {
        var request = new CompleteMultipartUploadRequest(bucket, key, uploadId);

        foreach (var part in parts)
        {
            request.SetPartNumberAndETag(part.PartNumber, part.ETag);
        }

        await Task.Run(() => _cosXml.CompleteMultiUpload(request), cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest(bucket, key, uploadId);
        await Task.Run(() => _cosXml.AbortMultiUpload(request), cancellationToken);
    }
}
