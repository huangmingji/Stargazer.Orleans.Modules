using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Users.Domain.Users;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations
{
    public class EfDbMigrationsContext(DbContextOptions options) : DbContext(options)
    {
        private DbSet<UserData> UserDatas {get;set;}

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