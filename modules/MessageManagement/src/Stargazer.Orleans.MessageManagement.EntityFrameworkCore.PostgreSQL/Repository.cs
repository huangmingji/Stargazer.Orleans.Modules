using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.MessageManagement.Domain;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL
{
    internal sealed class Repository<TEntity, TKey>(DbContext dbContext)
        : IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
        where TKey : notnull
    {
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;
        public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            TEntity? entity = await this.FindAsync(id, cancellationToken);
            if (entity != null)
            {
                dbContext.Set<TEntity>().Attach(entity);
                dbContext.Entry(entity).State = EntityState.Deleted;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            List<TEntity> entities = await this.FindListAsync(x => ids.Contains(x.Id), cancellationToken);
            foreach (var entity in entities)
            {
                dbContext.Set<TEntity>().Attach(entity);
                dbContext.Entry(entity).State = EntityState.Deleted;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = GetQueryable();
            var data = await dbContext.Set<TEntity>().Where(predicate).FirstOrDefaultAsync(cancellationToken);
            if (data == null)
            {
                throw new EntityNotFoundException();
            }
            return data;
        }

        public async Task<List<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetQueryable().ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<TEntity>().Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }
        
        public async Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
        }

        public List<TEntity> FindList(Expression<Func<TEntity, bool>> expression, int pageIndex, int pageSize,
                    Func<TEntity, Object>? orderBy = null, Func<TEntity, Object>? orderByDescending = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<TEntity> queryable = this.Where(expression);
            if (orderBy != null)
            {
                queryable = queryable.OrderBy(orderBy);
            }
            if (orderByDescending != null)
            {
                queryable = queryable.OrderByDescending(orderByDescending);
            }
            return queryable.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        public async Task<List<TEntity>> FindListAsync(string sql, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<TEntity>().FromSqlRaw(sql).ToListAsync(cancellationToken);
        }

        public async Task<List<TEntity>> FindListAsync(string sql, DbParameter[] dbParameter, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<TEntity>().FromSqlRaw(sql, dbParameter).ToListAsync(cancellationToken);
        }

        public IQueryable<TEntity> GetQueryable()
        {
            return dbContext.Set<TEntity>();
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            dbContext.Entry(entity).State = EntityState.Added;
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<List<TEntity>> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                dbContext.Entry(entity).State = EntityState.Added;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            return entities;
        }

        private IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQueryable().Where(predicate);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        
        public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var data = await GetQueryable().Where(x => x.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
            if (data == null)
            {
                throw new EntityNotFoundException();
            }
            return data;
        }

        public async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await GetQueryable().Where(x => x.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await Where(expression).AnyAsync(cancellationToken);
    }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await Where(expression).CountAsync(cancellationToken);
        }

        public async Task<(List<TEntity> Items, int Total)> FindListAsync(
            Expression<Func<TEntity, bool>>? predicate,
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool orderByDescending = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable();
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            if (orderBy != null)
            {
                query = orderByDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }
            
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            
        return (items, total);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress. Commit or rollback the existing transaction first.");
        }
        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }
        
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }
        
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
}