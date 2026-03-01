using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Stargazer.Orleans.Template.EntityFrameworkCore
{
    internal interface IDbContextProvider<out TDbContext> : IDisposable where TDbContext : DbContext
    {
        TDbContext GetDbContext();

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        void BeginTrans();
    }
}