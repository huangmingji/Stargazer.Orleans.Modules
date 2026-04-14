using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage;

namespace Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;

public class EfDbContext(DbContextOptions<EfDbContext> options) : DbContext(options)
{
    public DbSet<Bucket> Buckets => Set<Bucket>();
    public DbSet<ObjectInfo> Objects => Set<ObjectInfo>();
    public DbSet<MultipartUpload> MultipartUploads => Set<MultipartUpload>();
    public DbSet<BucketPolicy> BucketPolicies => Set<BucketPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Configure();
        base.OnModelCreating(modelBuilder);
    }
}
