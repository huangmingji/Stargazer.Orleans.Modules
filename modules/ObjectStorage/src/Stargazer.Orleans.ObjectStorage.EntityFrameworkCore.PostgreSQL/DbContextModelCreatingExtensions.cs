using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.ObjectStorage.Domain.Entities;

namespace Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;

public static class DbContextModelCreatingExtensions
{
    public static void Configure(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bucket>(entity =>
        {
            entity.ToTable("os_buckets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Acl).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.OwnerId);
        });

        modelBuilder.Entity<ObjectInfo>(entity =>
        {
            entity.ToTable("os_objects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.ETag).HasMaxLength(64);
            entity.Property(e => e.StorageClass).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => new { e.BucketId, e.Key }).IsUnique();
            entity.HasIndex(e => e.BucketId);
        });

        modelBuilder.Entity<MultipartUpload>(entity =>
        {
            entity.ToTable("os_multipart_uploads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UploadId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.Parts).HasColumnType("jsonb");
            entity.HasIndex(e => e.UploadId).IsUnique();
            entity.HasIndex(e => new { e.BucketId, e.Key, e.UploadId });
        });

        modelBuilder.Entity<BucketPolicy>(entity =>
        {
            entity.ToTable("os_bucket_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Principal).HasMaxLength(255);
            entity.Property(e => e.Effect).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PolicyType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Actions).HasColumnType("jsonb");
            entity.Property(e => e.Resource).HasMaxLength(500);
            entity.HasIndex(e => new { e.BucketId, e.Principal });
        });
    }
}