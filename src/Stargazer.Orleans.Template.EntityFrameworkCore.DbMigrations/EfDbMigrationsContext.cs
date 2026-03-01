using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Template.Domain.Users;

namespace Stargazer.Orleans.Template.EntityFrameworkCore.DbMigrations
{
    public class EfDbMigrationsContext(DbContextOptions<EfDbMigrationsContext> options) : DbContext(options)
    {
        private DbSet<Account> Accounts {get;set;}
        private DbSet<UserData> UserData {get;set;}

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