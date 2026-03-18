using Stargazer.Orleans.ObjectStorage.Domain;

namespace Stargazer.Orleans.ObjectStorage.Domain.Entities;

public class MultipartUpload : Entity<Guid>
{
    public Guid BucketId { get; set; }
    
    public string Key { get; set; } = "";
    
    public string UploadId { get; set; } = "";
    
    public string FileName { get; set; } = "";
    
    public string ContentType { get; set; } = "application/octet-stream";
    
    public long TotalSize { get; set; }
    
    public int PartSize { get; set; }
    
    public int TotalParts { get; set; }
    
    public int UploadedParts { get; set; }
    
    public UploadStatus Status { get; set; } = UploadStatus.InProgress;
    
    public string Metadata { get; set; } = "{}";
    
    public Guid InitiatedBy { get; set; }
    
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public List<UploadPart> Parts { get; set; } = new();
}

public class UploadPart
{
    public int PartNumber { get; set; }
    
    public string ETag { get; set; } = "";
    
    public long Size { get; set; }
}

public enum UploadStatus
{
    InProgress,
    Completed,
    Aborted,
    Failed
}
