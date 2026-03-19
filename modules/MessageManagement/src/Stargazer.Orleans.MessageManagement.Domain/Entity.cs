namespace Stargazer.Orleans.MessageManagement.Domain;

public abstract class Entity<TKey> : IEntity<TKey> where TKey : notnull
{

    public virtual TKey Id { get; set; }

    /// <summary>
    /// 新增人员
    /// </summary>
    public TKey CreatorId { get; set; }

    /// <summary>
    /// 新增时间
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    public TKey LastModifierId { get; set; }
        
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime LastModifyTime { get; set; } = DateTime.Now;
}
