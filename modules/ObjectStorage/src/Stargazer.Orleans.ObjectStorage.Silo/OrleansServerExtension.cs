using System.Net;
using Orleans.Configuration;
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

        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项
            siloBuilder.UseRedisClustering(configuration.GetConnectionString("Redis"))
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "object-storage";
                options.ServiceId = "orleans-app";
            }).AddAdoNetGrainStorageAsDefault(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("ObjectStorage");
            }).AddAdoNetGrainStorage("OrleansStore", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("ObjectStorage");
            }).Configure<EndpointOptions>(options =>
            {
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 30000);
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = 11111;
                options.GatewayPort = 30000;
            }).ConfigureLogging(logging => logging.AddConsole());
        });
        
        // 注册 Storage Provider
        builder.Services.AddSingleton(storageSettings);
        builder.Services.AddSingleton<IStorageProviderFactory, StorageProviderFactory>();
        builder.Services.AddScoped<IStorageProvider>(sp => sp.GetRequiredService<IStorageProviderFactory>().GetDefaultProvider());
        
        return builder;
    }
}