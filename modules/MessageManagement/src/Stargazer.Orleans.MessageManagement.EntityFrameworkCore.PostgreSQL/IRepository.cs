using System.Data.Common;
using System.Linq.Expressions;
using Stargazer.Orleans.MessageManagement.Domain;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

public interface IRepository<TEntity, in TKey> 
    where TEntity : class, IEntity<TKey>, new()
    where TKey : notnull
{
    IQueryable<TEntity> GetQueryable();
    
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<List<TEntity>> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);

    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> FindAllAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    List<TEntity> FindList(Expression<Func<TEntity, bool>> expression, int pageIndex, int pageSize,
        Func<TEntity, Object>? orderBy = null, Func<TEntity, Object>? orderByDescending = null, CancellationToken cancellationToken = default);

    Task<List<TEntity>> FindListAsync(string sql, CancellationToken cancellationToken = default);

    Task<List<TEntity>> FindListAsync(string sql, DbParameter[] dbParameter, CancellationToken cancellationToken = default);
    
    Task<(List<TEntity> Items, int Total)> FindListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        int pageIndex,
        int pageSize,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default);
}