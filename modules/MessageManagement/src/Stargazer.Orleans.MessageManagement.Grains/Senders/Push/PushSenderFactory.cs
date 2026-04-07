using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

public class PushSenderFactory : IPushSender
{
    private readonly PushSettings _settings;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public PushSenderFactory(IConfiguration configuration, ILoggerFactory _loggerFactory, IHttpClientFactory httpClientFactory)
    {
        configuration.GetSection("Message:Push").Bind(_settings);
        _loggerFactory = _loggerFactory;
        _httpClientFactory = httpClientFactory;
    }
    public string ProviderName => "push_factory";

    private IPushSender GetProvider(string? providerName = null)
    {
        var name = providerName ?? _settings.DefaultProvider?.ToLower() ?? "jpush";
        var httpClient = _httpClientFactory.CreateClient("JPush");
        
        return name switch
        {
            "jpush" when _settings.JPush != null => new JPushSender(_settings.JPush, _loggerFactory.CreateLogger<JPushSender>(), httpClient),
            "umeng" when _settings.Umeng != null => new UmengSender(_settings.Umeng, _loggerFactory.CreateLogger<UmengSender>(), httpClient),
            _ when _settings.JPush != null => new JPushSender(_settings.JPush, _loggerFactory.CreateLogger<JPushSender>(), httpClient),
            _ => throw new NotSupportedException($"Push provider '{name}' is not configured or not supported.")
        };
    }

    public async Task<PushSendResult> SendAsync(
        PushRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        return await provider.SendAsync(request, cancellationToken);
    }

    public async Task<List<PushSendResult>> BatchSendAsync(
        List<PushRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        return await provider.BatchSendAsync(requests, cancellationToken);
    }
}
