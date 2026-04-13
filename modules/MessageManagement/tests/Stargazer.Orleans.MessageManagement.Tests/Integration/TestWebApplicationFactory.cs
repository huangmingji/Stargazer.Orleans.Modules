using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient HttpClient { get; private set; } = null!;
    public HttpClient UsersHttpClient { get; private set; } = null!;
    
    public const string UsersBaseUrl = "http://localhost:5079";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(sp =>
            {
                var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                
                config["ConnectionStrings:Message"] = "server=127.0.0.1;port=5432;Database=postgres;uid=postgres;pwd=123456";
                config["ConnectionStrings:Default"] = "server=127.0.0.1;port=5432;Database=postgres;uid=postgres;pwd=123456";
                config["ConnectionStrings:Redis"] = "127.0.0.1:6379";
                config["ConnectionStrings:Users"] = "server=127.0.0.1;port=5432;Database=postgres;uid=postgres;pwd=123456";
                
                return config;
            });
        });
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        
        UsersHttpClient = new HttpClient
        {
            BaseAddress = new Uri(UsersBaseUrl)
        };
        
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        UsersHttpClient?.Dispose();
        await Task.CompletedTask;
    }
}
