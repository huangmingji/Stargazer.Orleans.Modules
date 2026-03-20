using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

public class PushSenderFactory(PushSettings settings, ILoggerFactory loggerFactory) : IPushSender
{
    public string ProviderName => "push_factory";

    private IPushSender GetProvider(string? providerName = null)
    {
        var name = providerName ?? settings.DefaultProvider?.ToLower() ?? "jpush";
        
        return name switch
        {
            "jpush" when settings.JPush != null => new JPushSender(settings.JPush, loggerFactory.CreateLogger<JPushSender>()),
            "umeng" when settings.Umeng != null => new UmengSender(settings.Umeng, loggerFactory.CreateLogger<UmengSender>()),
            _ when settings.JPush != null => new JPushSender(settings.JPush, loggerFactory.CreateLogger<JPushSender>()),
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
