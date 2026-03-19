using Stargazer.Orleans.ObjectStorage.Domain.Entities;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Domain;

public class BucketEntityTests
{
    [Fact]
    public void NewBucket_HasDefaultValues()
    {
        var bucket = new Bucket
        {
            Id = Guid.NewGuid(),
            Name = "test-bucket"
        };

        Assert.True(bucket.IsActive);
        Assert.Equal(BucketAclType.Private, bucket.Acl);
        Assert.Equal(0, bucket.CurrentObjectCount);
        Assert.Equal(0, bucket.CurrentStorageSize);
    }

    [Fact]
    public void NewBucket_HasCreationTime()
    {
        var before = DateTime.UtcNow;
        var bucket = new Bucket
        {
            Id = Guid.NewGuid(),
            Name = "test-bucket",
            CreationTime = DateTime.UtcNow
        };
        var after = DateTime.UtcNow;

        Assert.True(bucket.CreationTime >= before && bucket.CreationTime <= after);
    }
}

public class ObjectInfoEntityTests
{
    [Fact]
    public void NewObjectInfo_HasDefaultValues()
    {
        var obj = new ObjectInfo
        {
            Id = Guid.NewGuid(),
            BucketId = Guid.NewGuid(),
            Key = "test.txt"
        };

        Assert.False(obj.IsDeleted);
    }

    [Fact]
    public void ObjectInfo_CanSetProperties()
    {
        var bucketId = Guid.NewGuid();
        var obj = new ObjectInfo
        {
            Id = Guid.NewGuid(),
            BucketId = bucketId,
            Key = "test/file.txt",
            FileName = "file.txt",
            ContentType = "text/plain",
            Size = 1024,
            ETag = "abc123",
            Metadata = "{}"
        };

        Assert.Equal(bucketId, obj.BucketId);
        Assert.Equal("test/file.txt", obj.Key);
        Assert.Equal("file.txt", obj.FileName);
        Assert.Equal("text/plain", obj.ContentType);
        Assert.Equal(1024, obj.Size);
        Assert.Equal("abc123", obj.ETag);
        Assert.Equal("{}", obj.Metadata);
    }
}

public class MultipartUploadEntityTests
{
    [Fact]
    public void NewMultipartUpload_HasDefaultValues()
    {
        var upload = new MultipartUpload
        {
            Id = Guid.NewGuid(),
            BucketId = Guid.NewGuid(),
            Key = "test.txt",
            UploadId = "upload-123"
        };

        Assert.Equal(UploadStatus.InProgress, upload.Status);
        Assert.Equal(0, upload.UploadedParts);
        Assert.NotNull(upload.Parts);
    }

    [Fact]
    public void MultipartUpload_CanAddParts()
    {
        var upload = new MultipartUpload
        {
            Id = Guid.NewGuid(),
            BucketId = Guid.NewGuid(),
            Key = "test.txt",
            UploadId = "upload-123",
            Parts = new List<UploadPart>
            {
                new() { PartNumber = 1, ETag = "etag1", Size = 1024 },
                new() { PartNumber = 2, ETag = "etag2", Size = 2048 }
            }
        };

        Assert.Equal(2, upload.Parts.Count);
    }
}

public class BucketPolicyEntityTests
{
    [Fact]
    public void NewBucketPolicy_HasDefaultValues()
    {
        var policy = new BucketPolicy
        {
            Id = Guid.NewGuid(),
            BucketId = Guid.NewGuid(),
            Principal = "user:123"
        };

        Assert.True(policy.IsActive);
        Assert.Equal(EffectType.Allow, policy.Effect);
    }

    [Fact]
    public void BucketPolicy_CanSetDenyEffect()
    {
        var policy = new BucketPolicy
        {
            Id = Guid.NewGuid(),
            BucketId = Guid.NewGuid(),
            Principal = "user:123",
            Effect = EffectType.Deny,
            Actions = new List<string> { "Delete" }
        };

        Assert.Equal(EffectType.Deny, policy.Effect);
        Assert.Contains("Delete", policy.Actions);
    }
}
