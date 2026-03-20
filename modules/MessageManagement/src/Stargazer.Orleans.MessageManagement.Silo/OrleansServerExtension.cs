using System.Net;
using Orleans.Configuration;

namespace Stargazer.Orleans.MessageManagement.Silo;

public static class OrleansServerExtension
{
    internal static WebApplicationBuilder ConfigureOrleansServer(this WebApplicationBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();
        
        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项
            siloBuilder.UseRedisClustering(configuration.GetConnectionString("Redis"))
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "message";
                options.ServiceId = "orleans-app";
            }).AddAdoNetGrainStorageAsDefault(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("Message");
            }).AddAdoNetGrainStorage("OrleansStore", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("Message");
            }).Configure<EndpointOptions>(options =>
            {
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 30000);
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = 11111;
                options.GatewayPort = 30000;
            }).ConfigureLogging(logging => logging.AddConsole());
        });
        return builder;
    }
}
