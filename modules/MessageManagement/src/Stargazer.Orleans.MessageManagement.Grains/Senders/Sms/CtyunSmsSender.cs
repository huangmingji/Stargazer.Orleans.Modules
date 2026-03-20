using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class CtyunSmsSender : ISmsSender
{
    private readonly CtyunSmsSettings _settings;
    private readonly ILogger<CtyunSmsSender> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderName => "Ctyun";

    public CtyunSmsSender(CtyunSmsSettings settings, ILogger<CtyunSmsSender> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.RequestUrl)
        };
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedPhone = FormatPhoneNumber(phoneNumber);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var requestBody = new CtyunSmsRequest
            {
                Version = "v1.1",
                PhoneNumbers = new List<string> { formattedPhone },
                Signature = _settings.Signature,
                TemplateCode = templateCode,
                TemplateParam = templateParams
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var stringToSign = $"POST\n{_settings.RequestUrl}\n{jsonBody}\n{timestamp}";
            var signature = ComputeHmacSha256(stringToSign, _settings.AccessKeySecret);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/sms/sendSms");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Headers.Add("X-Auth-Key", _settings.AccessKeyId);
            request.Headers.Add("X-Auth-Signature", signature);
            request.Headers.Add("X-Auth-Timestamp", timestamp);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var ctyunResponse = JsonSerializer.Deserialize<CtyunSmsResponse>(responseContent);
                if (ctyunResponse?.Code == "0000")
                {
                    return new SmsSendResult
                    {
                        Success = true,
                        MessageId = ctyunResponse.Data?.SmsMsgId
                    };
                }

                _logger.LogError("Ctyun SMS failed: {Code} - {Message}", ctyunResponse?.Code, ctyunResponse?.Message);
                return new SmsSendResult
                {
                    Success = false,
                    ErrorCode = ctyunResponse?.Code,
                    ErrorMessage = ctyunResponse?.Message
                };
            }

            _logger.LogError("Ctyun SMS HTTP error: {StatusCode} - {Content}", response.StatusCode, responseContent);
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "HTTP_ERROR",
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ctyun SMS exception for phone {PhoneNumber}", phoneNumber);
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
            var result = await SendAsync(
                request.PhoneNumber,
                request.TemplateCode,
                request.TemplateParams,
                cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<SmsSendResult> SendTextAsync(
        string phoneNumber,
        string content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedPhone = FormatPhoneNumber(phoneNumber);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var requestBody = new CtyunSmsRequest
            {
                Version = "v1.1",
                PhoneNumbers = new List<string> { formattedPhone },
                Signature = _settings.Signature,
                Content = content
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var signature = ComputeHmacSha256(jsonBody, _settings.AccessKeySecret);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/sms/sendSms");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Headers.Add("X-Auth-Key", _settings.AccessKeyId);
            request.Headers.Add("X-Auth-Signature", signature);
            request.Headers.Add("X-Auth-Timestamp", timestamp);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var ctyunResponse = JsonSerializer.Deserialize<CtyunSmsResponse>(responseContent);
                if (ctyunResponse?.Code == "0000")
                {
                    return new SmsSendResult
                    {
                        Success = true,
                        MessageId = ctyunResponse.Data?.SmsMsgId
                    };
                }

                return new SmsSendResult
                {
                    Success = false,
                    ErrorCode = ctyunResponse?.Code,
                    ErrorMessage = ctyunResponse?.Message
                };
            }

            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "HTTP_ERROR",
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ctyun SMS text exception for phone {PhoneNumber}", phoneNumber);
            return new SmsSendResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    private static string FormatPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.StartsWith("+86"))
        {
            return phoneNumber;
        }
        if (phoneNumber.StartsWith("86") && phoneNumber.Length > 10)
        {
            return $"+86{phoneNumber[2..]}";
        }
        return $"+86{phoneNumber}";
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}

internal class CtyunSmsRequest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumbers")]
    public List<string> PhoneNumbers { get; set; } = new();

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    [JsonPropertyName("templateCode")]
    public string TemplateCode { get; set; } = string.Empty;

    [JsonPropertyName("templateParam")]
    public Dictionary<string, string>? TemplateParam { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

internal class CtyunSmsResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public CtyunSmsData? Data { get; set; }
}

internal class CtyunSmsData
{
    [JsonPropertyName("smsMsgId")]
    public string? SmsMsgId { get; set; }
}
