namespace Stargazer.Orleans.ObjectStorage.Domain;

public abstract class Entity<TKey> : IEntity<TKey> where TKey : notnull
{

    public virtual TKey Id { get; set; }

    public TKey CreatorId { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public TKey LastModifierId { get; set; }
        
    public DateTime LastModifyTime { get; set; } = DateTime.UtcNow;
}