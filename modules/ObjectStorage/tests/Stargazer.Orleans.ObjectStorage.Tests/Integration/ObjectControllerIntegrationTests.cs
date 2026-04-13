using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Integration;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class ObjectControllerIntegrationTests : IntegrationTestBase
{
    public ObjectControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DownloadObject_WithNonExistentBucket_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var response = await Client.GetAsync($"api/storage/object/{nonExistentBucketId}/test.txt");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CheckObjectExists_WithNonExistentObject_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, 
            $"api/storage/object/{nonExistentBucketId}/test.txt"));

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetObjectMetadata_WithNonExistentObject_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<ObjectMetadataDto>(
            $"api/storage/object/metadata/{nonExistentBucketId}/test.txt");

        Assert.False(success);
        Assert.Equal("object_not_found", errorCode);
    }

    [Fact]
    public async Task ListObjects_WithNonExistentBucket_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<List<ObjectMetadataDto>>(
            $"api/storage/object/{nonExistentBucketId}");

        Assert.False(success);
    }

    [Fact]
    public async Task GetSignedUrl_WithNonExistentObject_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<SignedUrlDto>(
            $"api/storage/object/signed-url/{nonExistentBucketId}/test.txt?expiry=00:01:00");

        Assert.False(success);
        Assert.Equal("object_not_found", errorCode);
    }

    [Fact]
    public async Task DeleteObject_WithNonExistentObject_ReturnsNotFound()
    {
        var nonExistentBucketId = Guid.NewGuid();

        var (success, data, errorCode) = await DeleteAsync<bool>(
            $"api/storage/object/{nonExistentBucketId}/test.txt");

        Assert.False(success);
        Assert.Equal("object_not_found", errorCode);
    }

    [Fact]
    public async Task CompleteWorkflow_CreateBucket_UploadObject_Download_Delete()
    {
        var bucketName = $"workflow_test_{Guid.NewGuid():N}";
        var bucket = new BucketDto
        {
            Name = bucketName,
            Description = "Workflow test bucket",
            Acl = "Private"
        };

        var (bucketSuccess, bucketData, _) = await PostAsync<BucketDto>("api/storage/bucket", bucket);
        Assert.True(bucketSuccess);
        Assert.NotNull(bucketData);
        var bucketId = bucketData.Id;

        var (listSuccess, listData, _) = await GetAsync<List<ObjectMetadataDto>>(
            $"api/storage/object/{bucketId}");
        Assert.True(listSuccess);
        Assert.NotNull(listData);

        var (metadataSuccess, _, metadataError) = await GetAsync<ObjectMetadataDto>(
            $"api/storage/object/metadata/{bucketId}/test.txt");
        Assert.False(metadataSuccess);
        Assert.Equal("object_not_found", metadataError);

        var (deleteSuccess, _, deleteError) = await DeleteAsync<bool>(
            $"api/storage/object/{bucketId}/test.txt");
        Assert.False(deleteSuccess);
        Assert.Equal("object_not_found", deleteError);
    }
}
