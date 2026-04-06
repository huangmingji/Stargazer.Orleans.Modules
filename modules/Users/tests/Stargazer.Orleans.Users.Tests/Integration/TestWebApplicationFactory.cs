using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient HttpClient { get; private set; } = null!;

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
                
                config["ConnectionStrings:Users"] = "server=127.0.0.1;port=5432;Database=postgres;uid=postgres;pwd=123456";
                config["ConnectionStrings:Default"] = "server=127.0.0.1;port=5432;Database=postgres;uid=postgres;pwd=123456";
                config["ConnectionStrings:Redis"] = "127.0.0.1:6379";
                
                return config;
            });
        });
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        await Task.CompletedTask;
    }
}