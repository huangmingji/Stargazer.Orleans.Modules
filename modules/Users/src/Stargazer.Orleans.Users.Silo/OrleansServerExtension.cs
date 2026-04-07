using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using StackExchange.Redis;
using Stargazer.Orleans.Users.Grains.Grains;

namespace Stargazer.Orleans.Users.Silo;

public static class OrleansServerExtension
{
    internal static WebApplicationBuilder ConfigureOrleansServer(this WebApplicationBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .Build();

        var orleansOptions = configuration.GetSection("Orleans").Get<OrleansOptions>() ?? new OrleansOptions();

        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项 - 统一集群
            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansOptions.ClusterId;
                options.ServiceId = orleansOptions.ServiceId;
            }).UseRedisClustering(configuration.GetConnectionString("Redis"))
            .AddRedisGrainStorageAsDefault(options =>
            {
                options.ConfigurationOptions =
                    ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
            })
            .AddRedisGrainStorage("OrleansStore", options =>
            {
                options.ConfigurationOptions =
                    ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
            })
            // .AddAdoNetGrainStorageAsDefault(options =>
            // {
            //     options.Invariant = "Npgsql";
            //     options.ConnectionString = configuration.GetConnectionString("Users");
            // }).AddAdoNetGrainStorage("OrleansStore", options =>
            // {
            //     options.Invariant = "Npgsql";
            //     options.ConnectionString = configuration.GetConnectionString("Users");
            // })
            .Configure<EndpointOptions>(options =>
            {
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 30000);
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = 11111;
                options.GatewayPort = 30000;
            }).ConfigureLogging(logging => logging.AddConsole())
            .AddStartupTask<UsersSeedDataInitializer>();
        });

        return builder;
    }
}