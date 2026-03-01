using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Stargazer.Orleans.Template.EntityFrameworkCore.DbMigrations;

public class DbContextFactory : IDesignTimeDbContextFactory<EfDbMigrationsContext>
{
    public EfDbMigrationsContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<EfDbMigrationsContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"), 
                b=> b.MigrationsAssembly("Stargazer.Orleans.Template.EntityFrameworkCore.DbMigrations"));

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