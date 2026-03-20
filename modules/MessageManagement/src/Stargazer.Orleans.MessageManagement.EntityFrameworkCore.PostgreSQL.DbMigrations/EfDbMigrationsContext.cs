using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.MessageManagement.Domain;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL.DbMigrations
{
    public class EfDbMigrationsContext(DbContextOptions<EfDbMigrationsContext> options) : DbContext(options)
    {
        public DbSet<MessageRecord> MessageRecords => Set<MessageRecord>();
        public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
        public DbSet<ProviderConfig> ProviderConfigs => Set<ProviderConfig>();

        /// <summary>
        /// On the model creating.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Configure();
            base.OnModelCreating(modelBuilder);
        }
    }
}
