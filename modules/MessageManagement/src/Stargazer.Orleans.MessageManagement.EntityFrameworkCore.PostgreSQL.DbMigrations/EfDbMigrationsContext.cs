using Microsoft.EntityFrameworkCore;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL.DbMigrations
{
    public class EfDbMigrationsContext(DbContextOptions<EfDbMigrationsContext> options) : DbContext(options)
    {

        /// <summary>
        /// On the model creating.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Configure();
            base.OnModelCreating(modelBuilder);
            // foreach (var entityType in  new List<Type>() {typeof(Entity<>) })
            // {
            //     var assembly = Assembly.GetAssembly(entityType) ?? throw new NullReferenceException();
            //     var types  = assembly.DefinedTypes.AsEnumerable().Where(x => x.BaseType != null && (x.BaseType == entityType)).ToList();
            //     foreach (var type in types)
            //     {
            //         modelBuilder.Entity(type);
            //     }
            // }
        }
    }
}
