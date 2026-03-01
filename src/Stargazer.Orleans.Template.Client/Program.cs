// See https://aka.ms/new-console-template for more information

// 创建并配置客户端
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Stargazer.Orleans.Template.Grains.Abstractions.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
        // 配置集群选项
        client.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "production";
            options.ServiceId = "orleans-app";
        });
        
        // 使用AdoNet作为集群目录存储
        // client.UseAdoNetClustering(options =>
        // {
        //     options.Invariant = "Npgsql";
        //     options.ConnectionString = "server=127.0.0.1;port=5432;Database=orleans;uid=postgres;pwd=123456";
        // });
        
        // 使用Redis作为集群目录存储
        // 从环境变量中获取Redis连接字符串
        string redisConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__redis") ?? "127.0.0.1:6379";
        client.UseRedisClustering(redisConnectionString);
        
        // 使用Consul作为集群目录存储
        // client.UseConsulClientClustering(options =>
        // {
        //     var address = new Uri("http://127.0.0.1:8500");
        //     options.ConfigureConsulClient(address);
        // });
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime()
    .Build();

await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

var account = client.GetGrain<IAccountGrain>("admin");
var response = await account.RegisterAsync(new RegisterAccountInputDto()
{
    AccountName = "admin",
    Password = "password"
});

Console.WriteLine($"""
                   account register result: account-{response.AccountName}  password-{response.Password}

                   Press any key to exit...
                   """);

Console.ReadKey();
await host.StopAsync();