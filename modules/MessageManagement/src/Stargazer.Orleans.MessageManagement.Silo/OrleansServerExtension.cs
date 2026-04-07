using System.Net;
using Microsoft.EntityFrameworkCore;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Email;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Push;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;
using IEmailSender = Stargazer.Orleans.MessageManagement.Grains.Senders.Email.IEmailSender;
using ISmsSender = Stargazer.Orleans.MessageManagement.Grains.Senders.Sms.ISmsSender;
using IPushSender = Stargazer.Orleans.MessageManagement.Grains.Senders.Push.IPushSender;

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
        
        var messageSettings = new MessageSettings();
        configuration.GetSection("Message").Bind(messageSettings);

        var orleansOptions = configuration.GetSection("Orleans").Get<OrleansOptions>() ?? new OrleansOptions();
        
        builder.Services.AddSingleton(messageSettings);
        builder.Services.AddSingleton(messageSettings.Email);
        builder.Services.AddSingleton(messageSettings.Sms);
        builder.Services.AddSingleton(messageSettings.Push);

        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
        builder.Services.AddScoped<ISmsSender, SmsSenderFactory>();
        builder.Services.AddScoped<IPushSender, PushSenderFactory>();

        builder.UseOrleans(siloBuilder =>
        {
            // 配置集群选项 - 统一集群
            siloBuilder.UseRedisClustering(configuration.GetConnectionString("Redis"))
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansOptions.ClusterId;
                options.ServiceId = orleansOptions.ServiceId;
            }).UseRedisReminderService(options =>
            {
                options.ConfigurationOptions = StackExchange.Redis.ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ?? "localhost:6379");
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
