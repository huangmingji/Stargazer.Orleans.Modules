using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Stargazer.Orleans.Template.EntityFrameworkCore.DbMigrations;

public static class EntityFramworkCoreExtensions
{
    public static void MigrateDatabase(this IServiceCollection serviceCollection)
    {
        IConfiguration? configuration = serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
        serviceCollection.AddDbContextFactory<EfDbMigrationsContext>(options =>
        {
            options.UseNpgsql(configuration?.GetConnectionString("Default"));
        }, ServiceLifetime.Scoped);
        serviceCollection.BuildServiceProvider().GetService<EfDbMigrationsContext>()?.Database.Migrate();
    }
}