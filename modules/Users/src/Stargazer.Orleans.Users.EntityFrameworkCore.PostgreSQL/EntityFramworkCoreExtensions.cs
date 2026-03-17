using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;

public static class EntityFramworkCoreExtensions
{
    public static IServiceCollection UseEntityFramworkCore(this IServiceCollection serviceCollection)
    {
        IConfiguration? configuration = serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
        serviceCollection.AddScoped<DbContext,EfDbContext>();
        serviceCollection.AddDbContext<EfDbContext>(
            options => { options.UseNpgsql(configuration?.GetConnectionString("Users")); }, ServiceLifetime.Scoped);
        serviceCollection.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        return serviceCollection;
    }
}