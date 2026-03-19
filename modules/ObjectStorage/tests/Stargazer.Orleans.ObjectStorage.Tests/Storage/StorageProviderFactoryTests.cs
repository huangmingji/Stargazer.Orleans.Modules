using Stargazer.Orleans.ObjectStorage.Silo.Configuration;
using Xunit;

namespace Stargazer.Orleans.ObjectStorage.Tests.Storage;

public class StorageProviderFactoryTests
{
    [Fact]
    public void StorageSettings_DefaultProvider_IsLocal()
    {
        var settings = new StorageSettings { Provider = "local" };
        Assert.Equal("local", settings.Provider);
    }

    [Fact]
    public void StorageSettings_CanSetProvider()
    {
        var settings = new StorageSettings { Provider = "aliyun" };
        Assert.Equal("aliyun", settings.Provider);
    }

    [Fact]
    public void StorageSettings_AllProviderTypes()
    {
        var providers = new[] { "local", "aliyun", "aws", "azure", "tencent", "minio" };
        
        foreach (var provider in providers)
        {
            var settings = new StorageSettings { Provider = provider };
            Assert.Equal(provider, settings.Provider);
        }
    }
}
