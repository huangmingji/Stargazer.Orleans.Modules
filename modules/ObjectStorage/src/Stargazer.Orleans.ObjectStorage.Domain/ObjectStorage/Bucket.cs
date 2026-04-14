namespace Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage;

public class Bucket : Entity<Guid>
{
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public BucketAclType Acl { get; set; } = BucketAclType.Private;
    
    public long MaxObjectSize { get; set; } = 100 * 1024 * 1024; // 100MB
    
    public long MaxObjectCount { get; set; } = 1000;
    
    public long CurrentObjectCount { get; set; }
    
    public long CurrentStorageSize { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifyTime { get; set; }
}

public enum BucketAclType
{
    Private,
    PublicRead,
    PublicReadWrite,
    Custom
}
