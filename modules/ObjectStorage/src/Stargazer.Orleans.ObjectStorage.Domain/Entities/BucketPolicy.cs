using Stargazer.Orleans.ObjectStorage.Domain;

namespace Stargazer.Orleans.ObjectStorage.Domain.Entities;

public class BucketPolicy : Entity<Guid>
{
    public Guid BucketId { get; set; }
    
    public PolicyType Type { get; set; }
    
    public string Principal { get; set; } = ""; // userId or role
    
    public string Resource { get; set; } = ""; // bucket or bucket/*
    
    public List<string> Actions { get; set; } = new();
    
    public EffectType Effect { get; set; } = EffectType.Allow;
    
    public int Priority { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}

public enum PolicyType
{
    User,
    Role,
    IpAddress
}

public enum EffectType
{
    Allow,
    Deny
}
