using Stargazer.Orleans.ObjectStorage.Silo.Configuration;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Configuration;

public class StorageSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new StorageSettings();

        Assert.Equal("local", settings.Provider);
        Assert.Equal(60, settings.DefaultExpiryMinutes);
        Assert.NotNull(settings.Local);
        Assert.NotNull(settings.Aliyun);
        Assert.NotNull(settings.Aws);
        Assert.NotNull(settings.Azure);
        Assert.NotNull(settings.Tencent);
        Assert.NotNull(settings.Minio);
    }

    [Fact]
    public void LocalStorageSettings_DefaultValues()
    {
        var settings = new LocalStorageSettings();

        Assert.Equal("/data/objects", settings.BasePath);
    }

    [Fact]
    public void AliyunStorageSettings_DefaultValues()
    {
        var settings = new AliyunStorageSettings();

        Assert.Equal("", settings.Endpoint);
        Assert.Equal("", settings.AccessKeyId);
        Assert.Equal("", settings.AccessKeySecret);
        Assert.Equal("", settings.BucketName);
    }

    [Fact]
    public void AwsStorageSettings_DefaultValues()
    {
        var settings = new AwsStorageSettings();

        Assert.Equal("us-east-1", settings.Region);
        Assert.Equal("", settings.AccessKeyId);
        Assert.Equal("", settings.SecretAccessKey);
        Assert.Equal("", settings.BucketName);
    }

    [Fact]
    public void AzureStorageSettings_DefaultValues()
    {
        var settings = new AzureStorageSettings();

        Assert.Equal("", settings.ConnectionString);
        Assert.Equal("", settings.ContainerName);
    }

    [Fact]
    public void TencentStorageSettings_DefaultValues()
    {
        var settings = new TencentStorageSettings();

        Assert.Equal("ap-guangzhou", settings.Region);
        Assert.Equal("", settings.SecretId);
        Assert.Equal("", settings.SecretKey);
        Assert.Equal("", settings.BucketName);
    }

    [Fact]
    public void MinioStorageSettings_DefaultValues()
    {
        var settings = new MinioStorageSettings();

        Assert.Equal("localhost:9000", settings.Endpoint);
        Assert.Equal("", settings.AccessKey);
        Assert.Equal("", settings.SecretKey);
        Assert.Equal("", settings.BucketName);
        Assert.False(settings.UseSSL);
    }
}
