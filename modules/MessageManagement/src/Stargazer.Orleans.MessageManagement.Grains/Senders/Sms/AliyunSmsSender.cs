using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class AliyunSmsSender(AliyunSmsSettings settings, ILogger<AliyunSmsSender> logger)
    : ISmsSender
{
    private readonly AliyunSmsSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<AliyunSmsSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string ProviderName => "Aliyun";

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        
        if (string.IsNullOrEmpty(_settings.AccessKeyId))
        {
            _logger.LogWarning("Aliyun SMS not configured - stub implementation");
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "Aliyun SMS sender is not configured"
            };
        }
        
        _logger.LogInformation("Aliyun SMS stub - would send to {PhoneNumber}", phoneNumber);
        return new SmsSendResult
        {
            Success = true,
            MessageId = Guid.NewGuid().ToString()
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
