using System.Net;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;

public interface IStorageProvider
{
    string ProviderName { get; }
    
    Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default);
    
    Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default);
    
    Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default);
    
    Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default);
    
    Task<ObjectMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default);
    
    Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default);
    
    Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default);
    
    Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default);
    
    Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default);
    
    Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default);
    
    // Multipart upload
    Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default);
    
    Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default);
    
    Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default);
    
    Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default);
}

public class ObjectMetadata
{
    public string ContentType { get; set; } = "application/octet-stream";
    public long ContentLength { get; set; }
    public string ETag { get; set; } = "";
    public DateTime? LastModified { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string CacheControl { get; set; } = "";
    public string ContentDisposition { get; set; } = "";
    public string ContentEncoding { get; set; } = "";
    public DateTime? Expires { get; set; }
    public string StorageClass { get; set; } = "Standard";
}

public class ObjectInfo
{
    public string Key { get; set; } = "";
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public string ETag { get; set; } = "";
    public string StorageClass { get; set; } = "Standard";
    public bool IsDirectory { get; set; }
}

public class PartETag
{
    public int PartNumber { get; set; }
    public string ETag { get; set; } = "";
}
