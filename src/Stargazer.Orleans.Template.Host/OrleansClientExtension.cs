using Orleans.Configuration;

namespace Stargazer.Orleans.Template.Host;

internal static class OrleansClientExtension
{
    internal static void ConfigureOrleansClient(this WebApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;
        builder.Host.UseOrleansClient(client =>
            {
                // client.UseLocalhostClustering();
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
                string redisConnectionString = configuration.GetConnectionString("Redis") ?? "127.0.0.1:6379";
                client.UseRedisClustering(redisConnectionString);

                // 使用Consul作为集群目录存储
                // client.UseConsulClientClustering(options =>
                // {
                //     var address = new Uri("http://127.0.0.1:8500");
                //     options.ConfigureConsulClient(address);
                // });
            })
            .ConfigureLogging(logging => logging.AddConsole());
    } 
}