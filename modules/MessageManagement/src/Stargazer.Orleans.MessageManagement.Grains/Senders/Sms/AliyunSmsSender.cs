using System.Text.Json;
using AlibabaCloud.SDK.Dysmsapi20170525;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using AlibabaCloud.OpenApiClient.Models;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class AliyunSmsSender : ISmsSender
{
    private readonly AliyunSmsSettings _settings;
    private readonly ILogger<AliyunSmsSender> _logger;
    private readonly Client _client;

    public string ProviderName => "aliyun";

    public AliyunSmsSender(AliyunSmsSettings settings, ILogger<AliyunSmsSender> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = CreateClient();
    }
    
    private Client CreateClient()
    {
        var config = new Config
        {
            AccessKeyId = _settings.AccessKeyId,
            AccessKeySecret = _settings.AccessKeySecret,
            Endpoint = _settings.Endpoint,
        };
        return new Client(config);
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.AccessKeyId) || string.IsNullOrEmpty(_settings.AccessKeySecret))
        {
            _logger.LogWarning("Aliyun SMS not configured - missing AccessKeyId or AccessKeySecret");
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "Aliyun SMS sender is not configured"
            };
        }

        try
        {
            var formattedPhone = PhoneNumberHelper.FormatForChina(phoneNumber);

            var request = new SendSmsRequest
            {
                PhoneNumbers = formattedPhone,
                SignName = _settings.SignName,
                TemplateCode = templateCode
            };

            if (templateParams != null && templateParams.Count > 0)
            {
                request.TemplateParam = JsonSerializer.Serialize(templateParams);
            }

            var response = await _client.SendSmsAsync(request);

            _logger.LogDebug("Aliyun SMS response: Code={Code}, Message={Message}, BizId={BizId}",
                response.Body?.Code, response.Body?.Message, response.Body?.BizId);

            if (response.Body?.Code == "OK")
            {
                return new SmsSendResult
                {
                    Success = true,
                    MessageId = response.Body.BizId
                };
            }

            _logger.LogError("Aliyun SMS failed: {Code} - {Message}", response.Body?.Code, response.Body?.Message);
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = response.Body?.Code,
                ErrorMessage = response.Body?.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aliyun SMS exception for phone {PhoneNumber}", phoneNumber);
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
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

    public async Task<SmsSendResult> SendTextAsync(
        string phoneNumber,
        string content,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Aliyun SMS does not support plain text sending directly - use template SMS");
        return await SendAsync(phoneNumber, string.Empty, new Dictionary<string, string> { { "content", content } }, cancellationToken);
    }
}
