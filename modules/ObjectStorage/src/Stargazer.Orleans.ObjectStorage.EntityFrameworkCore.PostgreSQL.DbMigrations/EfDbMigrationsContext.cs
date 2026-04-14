using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage;

namespace Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL.DbMigrations
{
    public class EfDbMigrationsContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Bucket> Buckets => Set<Bucket>();
        public DbSet<ObjectInfo> Objects => Set<ObjectInfo>();
        public DbSet<MultipartUpload> MultipartUploads => Set<MultipartUpload>();
        public DbSet<BucketPolicy> BucketPolicies => Set<BucketPolicy>();

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