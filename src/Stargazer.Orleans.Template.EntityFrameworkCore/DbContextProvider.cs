using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Stargazer.Orleans.Template.EntityFrameworkCore
{
    internal class DbContextProvider<TDbContext>
        : IDbContextProvider<TDbContext>
        where TDbContext : DbContext
    {
        [ThreadStatic]
        private TDbContext _dbContext;
        [ThreadStatic]
        private IDbContextTransaction? _dbContextTransaction;

        private IDbContextFactory<TDbContext> _contextFactory;

        internal DbContextProvider(IDbContextFactory<TDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _dbContext ??= contextFactory.CreateDbContext();
        }

        public TDbContext GetDbContext()
        {
            return _dbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContextTransaction == null)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public void BeginTrans()
        {
            _dbContextTransaction ??= GetDbContext().Database.BeginTransaction();
        }

        public void Dispose()
        {
            try
            {
                if (_dbContextTransaction != null)
                {
                    _dbContextTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                if (_dbContextTransaction != null)
                {
                    _dbContextTransaction.Rollback();
                }
                throw;
            }
            finally
            {
                _dbContextTransaction?.Dispose();
                _dbContext.Dispose();
            }
        }
    }
}