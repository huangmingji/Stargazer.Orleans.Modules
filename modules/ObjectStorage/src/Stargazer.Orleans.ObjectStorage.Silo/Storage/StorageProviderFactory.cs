using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public interface IStorageProviderFactory
{
    IStorageProvider CreateProvider(string? providerName = null);
    IStorageProvider GetDefaultProvider();
}

public class StorageProviderFactory : IStorageProviderFactory
{
    private readonly StorageSettings _settings;
    private readonly Dictionary<string, IStorageProvider> _providers = new();
    private readonly IStorageProvider _defaultProvider;

    public StorageProviderFactory(StorageSettings settings)
    {
        _settings = settings;
        
        // Register providers
        _providers["local"] = new LocalStorageProvider(settings.Local);
        _providers["aliyun"] = new AliyunOssProvider(settings.Aliyun);
        _providers["aws"] = new AwsS3Provider(settings.Aws);
        _providers["azure"] = new AzureBlobProvider(settings.Azure);
        _providers["tencent"] = new TencentCosProvider(settings.Tencent);
        _providers["minio"] = new MinioProvider(settings.Minio);
        
        _defaultProvider = _providers.GetValueOrDefault(settings.Provider.ToLower(), _providers["local"]);
    }

    public IStorageProvider CreateProvider(string? providerName = null)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return _defaultProvider;
        }
        
        return _providers.GetValueOrDefault(providerName.ToLower(), _defaultProvider);
    }

    public IStorageProvider GetDefaultProvider()
    {
        return _defaultProvider;
    }
}
