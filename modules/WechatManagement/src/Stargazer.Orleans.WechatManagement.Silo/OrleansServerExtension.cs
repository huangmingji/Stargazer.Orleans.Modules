using System.Net;
using Orleans.Configuration;
using StackExchange.Redis;

namespace Stargazer.Orleans.WechatManagement.Silo;

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
                })
                // .UseRedisClustering(configuration.GetConnectionString("Redis"))
                // .UseRedisReminderService(options =>
                // {
                //     options.ConfigurationOptions = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
                // }).AddRedisGrainStorageAsDefault(options =>
                // {
                //     options.ConfigurationOptions =
                //         ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
                // })
                // .AddRedisGrainStorage("OrleansStore", options =>
                // {
                //     options.ConfigurationOptions =
                //         ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
                // })
                // .AddRedisStreams("OrleansStreams", options => {
                //     options.ConfigureOptions = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
                // })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = configuration.GetConnectionString("Wechat");
                })
                .AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = configuration.GetConnectionString("Wechat");
                }).AddAdoNetGrainStorage("OrleansStore", options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = configuration.GetConnectionString("Wechat");
                })
                // https://github.com/dotnet/orleans/blob/main/src/AdoNet/Orleans.Streaming.AdoNet/README.md
                .AddAdoNetStreams(
                    name: "OrleansStreams",
                    configureOptions: options =>
                    {
                        options.Invariant = "Npgsql";
                        options.ConnectionString = configuration.GetConnectionString("Wechat");
                    })
                .Configure<EndpointOptions>(options =>
                {
                    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, orleansOptions.SiloPort);
                    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, orleansOptions.GatewayPort);
                    options.AdvertisedIPAddress = IPAddress.Loopback;
                    options.SiloPort = orleansOptions.SiloPort;
                    options.GatewayPort = orleansOptions.GatewayPort;
                }).ConfigureLogging(logging => logging.AddConsole());
        });

        return builder;
    }
}