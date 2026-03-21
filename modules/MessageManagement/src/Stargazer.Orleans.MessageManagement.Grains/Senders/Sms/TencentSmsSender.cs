using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Sms.V20210111;
using TencentCloud.Sms.V20210111.Models;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class TencentSmsSender : ISmsSender
{
    private readonly TencentSmsSettings _settings;
    private readonly ILogger<TencentSmsSender> _logger;
    private readonly SmsClient? _client;

    public string ProviderName => "tencent";

    public TencentSmsSender(TencentSmsSettings settings, ILogger<TencentSmsSender> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(_settings.SecretId) || string.IsNullOrEmpty(_settings.SecretKey))
        {
            _client = null;
            return;
        }

        var cred = new Credential
        {
            SecretId = _settings.SecretId,
            SecretKey = _settings.SecretKey
        };

        var clientProfile = new ClientProfile
        {
            SignMethod = ClientProfile.SIGN_TC3SHA256
        };

        var httpProfile = new HttpProfile
        {
            ReqMethod = "POST",
            Timeout = 10,
            Endpoint = "sms.tencentcloudapi.com"
        };

        clientProfile.HttpProfile = httpProfile;

        _client = new SmsClient(cred, _settings.Region ?? "ap-guangzhou", clientProfile);
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("Tencent SMS not configured - missing SecretId or SecretKey");
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "Tencent SMS sender is not configured"
            };
        }

        try
        {
            var formattedPhone = FormatPhoneNumber(phoneNumber);
            var templateId = string.IsNullOrEmpty(templateCode) ? _settings.TemplateId : templateCode;

            var req = new SendSmsRequest
            {
                SmsSdkAppId = _settings.SdkAppId,
                SignName = _settings.SmsSign,
                TemplateId = templateId,
                PhoneNumberSet = new[] { formattedPhone }
            };

            if (templateParams != null && templateParams.Count > 0)
            {
                req.TemplateParamSet = templateParams.Values.ToArray();
            }

            var resp = await _client.SendSms(req);

            _logger.LogDebug("Tencent SMS response: {Response}", AbstractModel.ToJsonString(resp));

            if (resp.SendStatusSet != null && resp.SendStatusSet.Length > 0)
            {
                var status = resp.SendStatusSet[0];
                if (status.Code == "Ok")
                {
                    return new SmsSendResult
                    {
                        Success = true,
                        MessageId = status.SerialNo
                    };
                }

                _logger.LogError("Tencent SMS failed: {Code} - {Message}", status.Code, status.Message);
                return new SmsSendResult
                {
                    Success = false,
                    ErrorCode = status.Code,
                    ErrorMessage = status.Message
                };
            }

            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "NO_RESPONSE",
                ErrorMessage = "No response from Tencent SMS"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tencent SMS exception for phone {PhoneNumber}", phoneNumber);
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
        _logger.LogWarning("Tencent SMS does not support plain text sending directly - use template SMS");
        return await SendAsync(phoneNumber, _settings.TemplateId, new Dictionary<string, string> { { "content", content } }, cancellationToken);
    }

    private static string FormatPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return phoneNumber;
        }

        if (phoneNumber.StartsWith("+86"))
        {
            return phoneNumber;
        }
        if (phoneNumber.StartsWith("86") && phoneNumber.Length > 10)
        {
            return $"+{phoneNumber}";
        }
        if (!phoneNumber.StartsWith("+"))
        {
            return $"+86{phoneNumber}";
        }
        return phoneNumber;
    }
}
