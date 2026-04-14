using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Integration;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class BucketControllerIntegrationTests : IntegrationTestBase
{
    public BucketControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBucket_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<BucketDto>($"api/storage/bucket/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("bucket_not_found", errorCode);
    }

    [Fact]
    public async Task GetBucketByName_WithNonExistentName_ReturnsNotFound()
    {
        var nonExistentName = $"nonexistent_{Guid.NewGuid():N}";

        var (success, data, errorCode) = await GetAsync<BucketDto>($"api/storage/bucket/name/{nonExistentName}");

        Assert.False(success);
        Assert.Equal("bucket_not_found", errorCode);
    }

    [Fact]
    public async Task CreateBucket_WithValidData_ReturnsSuccess()
    {
        var bucket = new BucketDto
        {
            Name = $"test_bucket_{Guid.NewGuid():N}",
            Description = "Test bucket",
            Acl = "Private",
            MaxObjectSize = 1024 * 1024 * 1024,
            MaxObjectCount = 10000
        };

        var (success, data, errorCode) = await PostAsync<BucketDto>("api/storage/bucket", bucket);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.NotEqual(Guid.Empty, data.Id);
        Assert.Equal(bucket.Name, data.Name);
    }

    [Fact]
    public async Task CreateBucket_WithDuplicateName_ReturnsBadRequest()
    {
        var bucketName = $"duplicate_bucket_{Guid.NewGuid():N}";
        var bucket = new BucketDto
        {
            Name = bucketName,
            Description = "Test bucket",
            Acl = "Private"
        };

        var (success1, _, _) = await PostAsync<BucketDto>("api/storage/bucket", bucket);
        Assert.True(success1);

        var (success2, _, errorCode) = await PostAsync<BucketDto>("api/storage/bucket", bucket);
        Assert.False(success2);
        Assert.Equal("bucket_exists", errorCode);
    }

    [Fact]
    public async Task GetUserBuckets_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<List<BucketDto>>("api/storage/bucket");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task UpdateBucket_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var bucket = new BucketDto
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        var (success, data, errorCode) = await PutAsync<BucketDto>($"api/storage/bucket/{nonExistentId}", bucket);

        Assert.False(success);
        Assert.Equal("bucket_not_found", errorCode);
    }

    [Fact]
    public async Task DeleteBucket_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var (success, data, errorCode) = await DeleteAsync<bool>($"api/storage/bucket/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("bucket_not_found", errorCode);
    }

    [Fact]
    public async Task CheckAccess_WithValidId_ReturnsSuccess()
    {
        var bucketName = $"access_test_{Guid.NewGuid():N}";
        var bucket = new BucketDto
        {
            Name = bucketName,
            Description = "Access test bucket",
            Acl = "Private"
        };

        var (success, data, errorCode) = await PostAsync<BucketDto>("api/storage/bucket", bucket);
        Assert.True(success);
        Assert.NotNull(data);

        var (accessSuccess, accessData, _) = await GetAsync<bool>($"api/storage/bucket/{data.Id}/access?action=Read");

        Assert.True(accessSuccess);
        Assert.NotNull(accessData);
    }
}
