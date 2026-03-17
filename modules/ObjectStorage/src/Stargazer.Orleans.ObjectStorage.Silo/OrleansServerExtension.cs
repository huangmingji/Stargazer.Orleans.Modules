using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

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
        
        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项
            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "production";
                options.ServiceId = "orleans-app";
            });

            // siloBuilder.UseLocalhostClustering();

            // 配置集群选项
            // siloBuilder.UseAdoNetClustering(options =>
            // {
            //     options.Invariant = "Npgsql";
            //     options.ConnectionString = configuration.GetConnectionString("Orleans");
            // });

            // 配置集群选项
            siloBuilder.UseRedisClustering(configuration.GetConnectionString("Redis"));

            // 配置集群选项
            // siloBuilder.UseConsulSiloClustering(options =>
            // {
            //     var address = new Uri(configuration.GetConnectionString("Consul")??"");
            //     options.ConfigureConsulClient(address);
            // });

            siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("Orleans");
            });
            siloBuilder.AddAdoNetGrainStorage("OrleansStore", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = configuration.GetConnectionString("Orleans");
            });

            siloBuilder.Configure<EndpointOptions>(options =>
            {
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 30000);
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = 11111;
                options.GatewayPort = 30000;
            });
            // 配置日志，输出到控制台
            siloBuilder.ConfigureLogging(logging => logging.AddConsole());
        });
        return builder;
    }
}