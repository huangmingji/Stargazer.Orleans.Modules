namespace Stargazer.Orleans.MessageManagement.Domain;

public interface IEntity<out TKey> where TKey : notnull
{
    /// <summary>
    /// 主键
    /// </summary>
    /// <value>The identifier.</value>
    TKey Id { get; }
}
