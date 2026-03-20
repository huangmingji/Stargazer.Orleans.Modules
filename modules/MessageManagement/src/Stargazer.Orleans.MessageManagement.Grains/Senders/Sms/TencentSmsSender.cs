using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class TencentSmsSender(TencentSmsSettings settings, ILogger<TencentSmsSender> logger)
    : ISmsSender
{
    private readonly TencentSmsSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<TencentSmsSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string ProviderName => "Tencent";

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        
        _logger.LogWarning("Tencent SMS not configured - stub implementation");
        return new SmsSendResult
        {
            Success = false,
            ErrorCode = "NOT_CONFIGURED",
            ErrorMessage = "Tencent SMS sender is not configured"
        };
    }

    public async Task<List<SmsSendResult>> BatchSendAsync(
        List<SmsSendRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var results = new List<SmsSendResult>();
        foreach (var request in requests)
        {
            var result = await SendAsync(request.PhoneNumber, request.TemplateCode, request.TemplateParams, cancellationToken);
            results.Add(result);
        }
        return results;
    }

    public Task<SmsSendResult> SendTextAsync(string phoneNumber, string content, CancellationToken cancellationToken = default)
    {
        return SendAsync(phoneNumber, string.Empty, new Dictionary<string, string> { { "content", content } }, cancellationToken);
    }
}
