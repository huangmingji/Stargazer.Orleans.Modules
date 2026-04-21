using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL.DbMigrations;

public class DbContextFactory : IDesignTimeDbContextFactory<EfDbMigrationsContext>
{
    public EfDbMigrationsContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<EfDbMigrationsContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"), 
                b=> b.MigrationsAssembly("Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL.DbMigrations"));

        return new EfDbMigrationsContext(builder.Options);
    }
        
    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
