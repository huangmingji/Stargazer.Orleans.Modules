using Stargazer.Orleans.ObjectStorage.Domain;

namespace Stargazer.Orleans.ObjectStorage.Domain.Entities;

public class ObjectInfo : Entity<Guid>
{
    public Guid BucketId { get; set; }
    
    public string Key { get; set; } = "";
    
    public string FileName { get; set; } = "";
    
    public string ContentType { get; set; } = "application/octet-stream";
    
    public long Size { get; set; }
    
    public string ETag { get; set; } = "";
    
    public string StorageClass { get; set; } = "Standard";
    
    public string CacheControl { get; set; } = "";
    
    public string ContentDisposition { get; set; } = "";
    
    public string ContentEncoding { get; set; } = "";
    
    public string Metadata { get; set; } = "{}"; // JSON string for custom metadata
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime? LastModified { get; set; }
    
    public Guid? UploadedBy { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
