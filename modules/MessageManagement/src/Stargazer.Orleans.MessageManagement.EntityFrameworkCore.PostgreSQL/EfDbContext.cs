using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.MessageManagement.Domain;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

public class EfDbContext(DbContextOptions<EfDbContext> options) : DbContext(options)
{
    public DbSet<MessageRecord> MessageRecords => Set<MessageRecord>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<ProviderConfig> ProviderConfigs => Set<ProviderConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Configure();
        base.OnModelCreating(modelBuilder);
    }
}
