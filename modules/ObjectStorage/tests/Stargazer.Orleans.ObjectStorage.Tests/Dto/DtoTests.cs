using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Dto;

public class DtoTests
{
    [Fact]
    public void BucketDto_CanSetProperties()
    {
        var dto = new BucketDto
        {
            Id = Guid.NewGuid(),
            Name = "test-bucket",
            Description = "Test description",
            Acl = "Private",
            MaxObjectSize = 1024,
            MaxObjectCount = 100,
            CurrentObjectCount = 10,
            CurrentStorageSize = 512,
            OwnerId = Guid.NewGuid(),
            IsActive = true
        };

        Assert.Equal("test-bucket", dto.Name);
        Assert.Equal("Private", dto.Acl);
        Assert.Equal(1024, dto.MaxObjectSize);
    }

    [Fact]
    public void ObjectMetadataDto_CanSetProperties()
    {
        var dto = new ObjectMetadataDto
        {
            Id = Guid.NewGuid(),
            Key = "test/file.txt",
            FileName = "file.txt",
            ContentType = "text/plain",
            Size = 1024,
            ETag = "abc123",
            Metadata = new Dictionary<string, string> { { "key", "value" } }
        };

        Assert.Equal("test/file.txt", dto.Key);
        Assert.Equal("text/plain", dto.ContentType);
        Assert.Equal(1024, dto.Size);
    }

    [Fact]
    public void UploadResultDto_CanSetProperties()
    {
        var dto = new UploadResultDto
        {
            Key = "test/file.txt",
            ETag = "abc123",
            Size = 1024,
            ContentType = "text/plain",
            Url = "/bucket/test/file.txt"
        };

        Assert.Equal("test/file.txt", dto.Key);
        Assert.Equal("/bucket/test/file.txt", dto.Url);
    }

    [Fact]
    public void SignedUrlDto_CanSetProperties()
    {
        var dto = new SignedUrlDto
        {
            Url = "https://example.com/signed-url",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        Assert.Contains("signed-url", dto.Url);
        Assert.True(dto.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void InitiateMultipartUploadResultDto_CanSetProperties()
    {
        var dto = new InitiateMultipartUploadResultDto
        {
            UploadId = "upload-123",
            Key = "test/file.txt"
        };

        Assert.Equal("upload-123", dto.UploadId);
        Assert.Equal("test/file.txt", dto.Key);
    }

    [Fact]
    public void UploadPartResultDto_CanSetProperties()
    {
        var dto = new UploadPartResultDto
        {
            PartNumber = 1,
            ETag = "etag-123"
        };

        Assert.Equal(1, dto.PartNumber);
        Assert.Equal("etag-123", dto.ETag);
    }

    [Fact]
    public void PartETagDto_CanSetProperties()
    {
        var dto = new PartETagDto
        {
            PartNumber = 1,
            ETag = "etag-123"
        };

        Assert.Equal(1, dto.PartNumber);
        Assert.Equal("etag-123", dto.ETag);
    }
}
