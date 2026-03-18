namespace Stargazer.Orleans.ObjectStorage.Domain;

public interface IEntity<out TKey> where TKey : notnull
{
    TKey Id { get; }
}