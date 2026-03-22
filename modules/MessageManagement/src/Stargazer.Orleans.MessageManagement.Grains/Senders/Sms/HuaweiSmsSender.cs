using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class HuaweiSmsSender : ISmsSender
{
    private readonly HuaweiSmsSettings _settings;
    private readonly ILogger<HuaweiSmsSender> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderName => "huawei";

    public HuaweiSmsSender(HuaweiSmsSettings settings, ILogger<HuaweiSmsSender> logger, HttpClient httpClient)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient;
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.Ak) || string.IsNullOrEmpty(_settings.Sk))
        {
            _logger.LogWarning("Huawei SMS not configured - missing Ak or Sk");
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "Huawei SMS sender is not configured"
            };
        }

        try
        {
            var formattedPhone = FormatPhoneNumber(phoneNumber);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var nonce = Guid.NewGuid().ToString("N");

            var requestBody = new Dictionary<string, object>
            {
                { "from", _settings.Sender },
                { "to", formattedPhone },
                { "templateId", templateCode }
            };

            if (templateParams != null && templateParams.Count > 0)
            {
                var templateParas = templateParams.Values.Select(p => p).ToList();
                requestBody.Add("templateParas", templateParas);
            }

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var url = $"{_settings.Endpoint}/sms/batchSendSms/v1";

            var stringToSign = $"POST\n{url}\n{jsonBody}\n{timestamp}";
            var signature = ComputeHmacSha256(stringToSign, _settings.Sk);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"WSSE realm=\"SDP\",profile=\"UsernameToken\",type=\"Appkey\"");
            request.Headers.Add("X-WSSE", BuildWsseHeader(_settings.Ak, nonce, timestamp, signature));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var huaweiResponse = JsonSerializer.Deserialize<HuaweiSmsResponse>(responseContent);
                var result = huaweiResponse?.Result?.FirstOrDefault();

                if (result != null && result.Status == "success")
                {
                    return new SmsSendResult
                    {
                        Success = true,
                        MessageId = result.SmsMsgId
                    };
                }

                _logger.LogError("Huawei SMS failed: Status={Status}, smsMsgId={SmsMsgId}",
                    result?.Status, result?.SmsMsgId);
                return new SmsSendResult
                {
                    Success = false,
                    ErrorCode = result?.Status ?? "UNKNOWN",
                    ErrorMessage = $"SMS status: {result?.Status}"
                };
            }

            _logger.LogError("Huawei SMS HTTP error: {StatusCode} - {Content}", response.StatusCode, responseContent);
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "HTTP_ERROR",
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Huawei SMS exception for phone {PhoneNumber}", phoneNumber);
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

    public Task<SmsSendResult> SendTextAsync(string phoneNumber, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Huawei SMS does not support plain text sending directly - use template SMS");
        return SendAsync(phoneNumber, string.Empty, new Dictionary<string, string> { { "content", content } }, cancellationToken);
    }

    private static string BuildWsseHeader(string appKey, string nonce, string timestamp, string passwordDigest)
    {
        return $"UsernameToken Username=\"{appKey}\", PasswordDigest=\"{passwordDigest}\", Nonce=\"{nonce}\", Created=\"{timestamp}\"";
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
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

internal class HuaweiSmsResponse
{
    [JsonPropertyName("result")]
    public List<HuaweiSmsResult>? Result { get; set; }
}

internal class HuaweiSmsResult
{
    [JsonPropertyName("originTo")]
    public string? OriginTo { get; set; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("smsMsgId")]
    public string? SmsMsgId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
