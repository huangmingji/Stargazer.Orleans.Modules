using System.Linq.Expressions;
using Stargazer.Orleans.Template.Domain;

namespace Stargazer.Orleans.Template.EntityFrameworkCore.Repositories
{
    public interface IAppRepository<TEntity, TKey> 
        where TEntity : class, IEntity<TKey>
        where TKey : notnull
    {
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        Task DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

        Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

        List<TEntity> FindList(Expression<Func<TEntity, bool>> expression, int pageIndex, int pageSize, 
                    Func<TEntity, Object>? orderBy = null, Func<TEntity, Object>? orderByDescending = null, CancellationToken cancellationToken = default);

        Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        
        Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);

        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    }
}