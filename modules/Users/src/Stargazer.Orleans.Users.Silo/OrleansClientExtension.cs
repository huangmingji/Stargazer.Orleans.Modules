// using Orleans.Configuration;

// namespace Stargazer.Orleans.Users.Host;

// internal static class OrleansClientExtension
// {
//     internal static void ConfigureOrleansClient(this WebApplicationBuilder builder)
//     {
//         IConfiguration configuration = builder.Configuration;
//         builder.Host.UseOrleansClient(client =>
//             {
//                 // client.UseLocalhostClustering();
//                 // 配置集群选项
//                 client.Configure<ClusterOptions>(options =>
//                 {
//                     options.ClusterId = "users";
//                     options.ServiceId = "orleans-app";
//                 });

//                 // 使用AdoNet作为集群目录存储
//                 client.UseAdoNetClustering(options =>
//                 {
//                     options.Invariant = "Npgsql";
//                     options.ConnectionString = configuration.GetConnectionString("Users");
//                 });

//                 // 使用Redis作为集群目录存储
//                 // 从环境变量中获取Redis连接字符串
//                 // string redisConnectionString = configuration.GetConnectionString("Redis") ?? "127.0.0.1:6379";
//                 // client.UseRedisClustering(redisConnectionString);
//             })
//             .ConfigureLogging(logging => logging.AddConsole());
//     } 
// }
