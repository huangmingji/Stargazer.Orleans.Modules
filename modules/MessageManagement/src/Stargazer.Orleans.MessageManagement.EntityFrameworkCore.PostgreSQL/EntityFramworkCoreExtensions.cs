using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

namespace Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;

public static class EntityFramworkCoreExtensions
{
    public static IServiceCollection UseObjectStorageEntityFramworkCore(this IServiceCollection serviceCollection)
    {
        IConfiguration? configuration = serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
        serviceCollection.AddScoped<DbContext,EfDbContext>();
        serviceCollection.AddDbContext<EfDbContext>(
            options => { options.UseNpgsql(configuration?.GetConnectionString("ObjectStorage")); }, ServiceLifetime.Scoped);
        serviceCollection.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        return serviceCollection;
    }
}