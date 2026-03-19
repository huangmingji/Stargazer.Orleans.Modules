using System.Text;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;
using Stargazer.Orleans.ObjectStorage.Silo.Storage;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Storage;

public class LocalStorageProviderTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalStorageProvider _provider;
    private readonly string _testBucket = "test-bucket";

    public LocalStorageProviderTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"objectstorage_tests_{Guid.NewGuid()}");
        var settings = new LocalStorageSettings { BasePath = _testBasePath };
        _provider = new LocalStorageProvider(settings);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            try
            {
                Directory.Delete(_testBasePath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void ProviderName_ReturnsLocal()
    {
        Assert.Equal("local", _provider.ProviderName);
    }

    [Fact]
    public async Task PutObjectAsync_CreatesFile()
    {
        var key = "test-file.txt";
        var content = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
        var metadata = new ObjectMetadata
        {
            ContentType = "text/plain",
            ContentLength = 11
        };

        await _provider.PutObjectAsync(_testBucket, key, content, metadata);

        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Assert.True(File.Exists(filePath));
        Assert.Equal("Hello World", await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task GetObjectAsync_ReturnsStream()
    {
        var key = "test-file.txt";
        var content = Encoding.UTF8.GetBytes("Hello World");
        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllBytesAsync(filePath, content);

        var result = await _provider.GetObjectAsync(_testBucket, key);
        
        using var reader = new StreamReader(result);
        Assert.Equal("Hello World", await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task GetObjectAsync_ThrowsWhenNotFound()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _provider.GetObjectAsync(_testBucket, "nonexistent.txt"));
    }

    [Fact]
    public async Task DeleteObjectAsync_RemovesFile()
    {
        var key = "test-file.txt";
        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "Hello");

        await _provider.DeleteObjectAsync(_testBucket, key);
        
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task ObjectExistsAsync_ReturnsTrue_WhenExists()
    {
        var key = "test-file.txt";
        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "Hello");

        var exists = await _provider.ObjectExistsAsync(_testBucket, key);
        
        Assert.True(exists);
    }

    [Fact]
    public async Task ObjectExistsAsync_ReturnsFalse_WhenNotExists()
    {
        var exists = await _provider.ObjectExistsAsync(_testBucket, "nonexistent.txt");
        Assert.False(exists);
    }

    [Fact]
    public async Task GetObjectMetadataAsync_ReturnsMetadata()
    {
        var key = "test-file.txt";
        var content = Encoding.UTF8.GetBytes("Hello World");
        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllBytesAsync(filePath, content);

        var metadata = await _provider.GetObjectMetadataAsync(_testBucket, key);
        
        Assert.Equal(11, metadata.ContentLength);
        Assert.NotNull(metadata.ETag);
    }

    [Fact]
    public async Task ListObjectsAsync_ReturnsFiles()
    {
        var bucketPath = Path.Combine(_testBasePath, _testBucket);
        Directory.CreateDirectory(bucketPath);
        
        await File.WriteAllTextAsync(Path.Combine(bucketPath, "file1.txt"), "content1");
        await File.WriteAllTextAsync(Path.Combine(bucketPath, "file2.txt"), "content2");

        var objects = await _provider.ListObjectsAsync(_testBucket, null);
        
        Assert.Equal(2, objects.Count);
    }

    [Fact]
    public async Task CreateBucketAsync_CreatesDirectory()
    {
        var bucketName = "new-bucket";

        await _provider.CreateBucketAsync(bucketName);
        
        var bucketPath = Path.Combine(_testBasePath, bucketName);
        Assert.True(Directory.Exists(bucketPath));
    }

    [Fact]
    public async Task DeleteBucketAsync_RemovesDirectory()
    {
        var bucketPath = Path.Combine(_testBasePath, _testBucket);
        Directory.CreateDirectory(bucketPath);

        await _provider.DeleteBucketAsync(_testBucket);
        
        Assert.False(Directory.Exists(bucketPath));
    }

    [Fact]
    public async Task BucketExistsAsync_ReturnsTrue_WhenExists()
    {
        var bucketPath = Path.Combine(_testBasePath, _testBucket);
        Directory.CreateDirectory(bucketPath);

        var exists = await _provider.BucketExistsAsync(_testBucket);
        
        Assert.True(exists);
    }

    [Fact]
    public async Task BucketExistsAsync_ReturnsFalse_WhenNotExists()
    {
        var exists = await _provider.BucketExistsAsync("nonexistent-bucket");
        Assert.False(exists);
    }

    [Fact]
    public async Task GetSignedUrlAsync_ReturnsFilePath()
    {
        var key = "test-file.txt";
        var filePath = Path.Combine(_testBasePath, _testBucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "Hello");

        var url = await _provider.GetSignedUrlAsync(_testBucket, key, TimeSpan.FromHours(1), HttpMethod.Get);
        
        Assert.StartsWith("file://", url);
        Assert.Contains(key.Replace("/", Path.DirectorySeparatorChar.ToString()), url);
    }
}
