using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Template.Domain;

namespace Stargazer.Orleans.Template.EntityFrameworkCore.Repositories
{
    internal sealed class EfCoreRepository<TEntity, TKey>(IRepository<TEntity, TKey> repository)
        : IAppRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
        where TKey : notnull
    {
        public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            await repository.DeleteAsync(id, cancellationToken);
        }

        public async Task DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            await repository.DeleteManyAsync(ids, cancellationToken);
        }

        public async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await repository.FindAsync(id, cancellationToken);
        }

        public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await repository.GetAsync(id,cancellationToken);
        }

        public List<TEntity> FindList(Expression<Func<TEntity, bool>> expression, int pageIndex, int pageSize,
                    Func<TEntity, Object>? orderBy = null, Func<TEntity, Object>? orderByDescending = null, CancellationToken cancellationToken = default)
        {
            return repository.FindList(expression, pageIndex, pageSize, orderBy, orderByDescending, cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await repository.CountAsync(expression, cancellationToken);
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return await repository.InsertAsync(entity, cancellationToken);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return await repository.UpdateAsync(entity, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await repository.AnyAsync(expression, cancellationToken);
        }

        public async Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await repository.FindListAsync(expression, cancellationToken);
        }
    }
}