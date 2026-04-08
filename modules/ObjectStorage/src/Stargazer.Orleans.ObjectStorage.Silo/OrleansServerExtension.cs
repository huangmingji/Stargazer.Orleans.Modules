using System.Net;
using Orleans.Configuration;
using StackExchange.Redis;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;
using Stargazer.Orleans.ObjectStorage.Silo.Storage;
using IStorageProvider = Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage.IStorageProvider;

namespace Stargazer.Orleans.ObjectStorage.Silo;

public static class OrleansServerExtension
{
    internal static WebApplicationBuilder ConfigureOrleansServer(this WebApplicationBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();
        
        var storageSettings = new StorageSettings();
        configuration.GetSection("Storage").Bind(storageSettings);

        var orleansOptions = configuration.GetSection("Orleans").Get<OrleansOptions>() ?? new OrleansOptions();

        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项 - 统一集群
            siloBuilder.UseRedisClustering(configuration.GetConnectionString("Redis"))
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansOptions.ClusterId;
                options.ServiceId = orleansOptions.ServiceId;
            }).AddRedisGrainStorageAsDefault(options =>
            {
                options.ConfigurationOptions =
                    ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
            }).AddRedisGrainStorage("OrleansStore", options =>
            {
                options.ConfigurationOptions =
                    ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
            })
            // .AddAdoNetGrainStorageAsDefault(options =>
            // {
            //     options.Invariant = "Npgsql";
            //     options.ConnectionString = configuration.GetConnectionString("ObjectStorage");
            // }).AddAdoNetGrainStorage("OrleansStore", options =>
            // {
            //     options.Invariant = "Npgsql";
            //     options.ConnectionString = configuration.GetConnectionString("ObjectStorage");
            // })
            .Configure<EndpointOptions>(options =>
            {
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, orleansOptions.SiloPort);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, orleansOptions.GatewayPort);
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = orleansOptions.SiloPort;
                options.GatewayPort = orleansOptions.GatewayPort;
            }).ConfigureLogging(logging => logging.AddConsole());
        });
        
        // 注册 Storage Provider
        builder.Services.AddSingleton(storageSettings);
        builder.Services.AddSingleton<IStorageProviderFactory, StorageProviderFactory>();
        builder.Services.AddScoped<IStorageProvider>(sp => sp.GetRequiredService<IStorageProviderFactory>().GetDefaultProvider());
        
        return builder;
    }
}