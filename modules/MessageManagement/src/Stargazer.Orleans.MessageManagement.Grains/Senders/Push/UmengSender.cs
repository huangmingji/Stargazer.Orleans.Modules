using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

public class UmengSender : IPushSender
{
    private readonly UmengSettings _settings;
    private readonly ILogger<UmengSender> _logger;
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://msgapi.umeng.com/api";

    public string ProviderName => "umeng";

    public UmengSender(UmengSettings settings, ILogger<UmengSender> logger, HttpClient httpClient)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient;
    }

    public async Task<PushSendResult> SendAsync(PushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var pushPayload = BuildPushPayload(request);
            var response = await ExecutePushAsync(pushPayload, cancellationToken);

            if (response.Ret == "SUCCESS")
            {
                _logger.LogInformation(
                    "Umeng message sent successfully. TaskId: {TaskId}, TargetType: {TargetType}",
                    response.TaskId,
                    request.TargetType);
                return new PushSendResult { Success = true, MessageId = response.TaskId };
            }
            else
            {
                var errorMsg = response.ErrorResponse?.ErrorMsg ?? response.ErrorResponse?.ErrorCode;
                _logger.LogWarning(
                    "Umeng message send failed. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    response.ErrorResponse?.ErrorCode,
                    errorMsg);
                return new PushSendResult
                {
                    Success = false,
                    ErrorMessage = $"{response.ErrorResponse?.ErrorCode}: {errorMsg}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Umeng send exception. TargetType: {TargetType}, Target: {Target}",
                request.TargetType, request.Targets.FirstOrDefault());
            return new PushSendResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<List<PushSendResult>> BatchSendAsync(List<PushRequest> requests, CancellationToken cancellationToken = default)
    {
        var results = new List<PushSendResult>();
        foreach (var request in requests)
        {
            var result = await SendAsync(request, cancellationToken);
            results.Add(result);
        }
        return results;
    }

    private UmengPushPayload BuildPushPayload(PushRequest request)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var type = request.TargetType.ToLowerInvariant() switch
        {
            "all" => "broadcast",
            "registration_id" => "unicast",
            "alias" => "customizedcast",
            "tag" => "groupcast",
            _ => "broadcast"
        };

        var payload = new UmengPushPayload
        {
            AppKey = _settings.AppKey,
            Timestamp = timestamp,
            type = type,
            Payload = new UmengPayload
            {
                DisplayType = "notification",
                Body = new UmengNotificationBody
                {
                    Ticker = request.Title,
                    Title = request.Title,
                    Text = request.Content
                },
                Extra = request.Extras ?? new Dictionary<string, string>()
            }
        };

        if (type == "unicast")
        {
            payload.DeviceTokens = request.Targets.FirstOrDefault() ?? "";
        }
        else if (type == "customizedcast")
        {
            payload.Alias = string.Join(",", request.Targets);
            payload.AliasType = "default";
        }
        else if (type == "groupcast")
        {
            payload.Filter = new UmengFilter
            {
                Where = new UmengFilterWhere
                {
                    Tag = request.Targets
                }
            };
        }

        if (request.ApnsProduction)
        {
            payload.ProductionMode = "true";
        }
        else
        {
            payload.ProductionMode = "false";
        }

        return payload;
    }

    private async Task<UmengResponse> ExecutePushAsync(UmengPushPayload payload, CancellationToken cancellationToken)
    {
        var sign = GenerateSignature(payload);
        var url = $"{ApiBaseUrl}/send?sign={sign}";
        
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogDebug("Umeng request: {Request}", json);
        
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        _logger.LogDebug("Umeng API response: {Response}", responseContent);
        
        try
        {
            return JsonSerializer.Deserialize<UmengResponse>(responseContent, JsonOptions) 
                   ?? new UmengResponse { Ret = "FAIL", ErrorResponse = new UmengError { ErrorCode = "-1", ErrorMsg = "Parse failed" } };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Umeng response: {Content}", responseContent);
            return new UmengResponse { Ret = "FAIL", ErrorResponse = new UmengError { ErrorCode = "-1", ErrorMsg = $"Parse error: {ex.Message}" } };
        }
    }

    private string GenerateSignature(UmengPushPayload payload)
    {
        var timestamp = payload.Timestamp;
        
        var sortedParams = new SortedDictionary<string, string>
        {
            ["application/json"] = "",
            ["appkey"] = payload.AppKey,
            ["timestamp"] = timestamp,
            ["type"] = payload.type
        };

        if (!string.IsNullOrEmpty(payload.DeviceTokens))
        {
            sortedParams["device_tokens"] = payload.DeviceTokens;
        }
        
        if (!string.IsNullOrEmpty(payload.Alias))
        {
            sortedParams["alias"] = payload.Alias;
            sortedParams["alias_type"] = payload.AliasType ?? "default";
        }

        var builder = new StringBuilder();
        builder.Append(_settings.AppMasterSecret);
        
        foreach (var kvp in sortedParams)
        {
            builder.Append(kvp.Key).Append(kvp.Value);
        }
        
        builder.Append(_settings.AppMasterSecret);
        
        var signString = builder.ToString();
        var md5Hash = MD5.HashData(Encoding.UTF8.GetBytes(signString));
        return Convert.ToHexString(md5Hash).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}

internal class UmengPushPayload
{
    [JsonPropertyName("appkey")]
    public string AppKey { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string type { get; set; } = string.Empty;

    [JsonPropertyName("device_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceTokens { get; set; }

    [JsonPropertyName("alias")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alias { get; set; }

    [JsonPropertyName("alias_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AliasType { get; set; }

    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UmengFilter? Filter { get; set; }

    [JsonPropertyName("payload")]
    public UmengPayload Payload { get; set; } = new();

    [JsonPropertyName("production_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProductionMode { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

internal class UmengFilter
{
    [JsonPropertyName("where")]
    public UmengFilterWhere? Where { get; set; }
}

internal class UmengFilterWhere
{
    [JsonPropertyName("tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Tag { get; set; }
}

internal class UmengPayload
{
    [JsonPropertyName("display_type")]
    public string DisplayType { get; set; } = "notification";

    [JsonPropertyName("body")]
    public UmengNotificationBody Body { get; set; } = new();

    [JsonPropertyName("extra")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Extra { get; set; }

    [JsonPropertyName("custom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Custom { get; set; }
}

internal class UmengNotificationBody
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Icon { get; set; }

    [JsonPropertyName("largeIcon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LargeIcon { get; set; }

    [JsonPropertyName("sound")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sound { get; set; }

    [JsonPropertyName("builder_id")]
    public string BuilderId { get; set; } = "0";

    [JsonPropertyName("play_vibrate")]
    public string PlayVibrate { get; set; } = "1";

    [JsonPropertyName("play_lights")]
    public string PlayLights { get; set; } = "1";

    [JsonPropertyName("play_sound")]
    public string PlaySound { get; set; } = "1";

    [JsonPropertyName("after_open")]
    public string AfterOpen { get; set; } = "go_app";
}

internal class UmengResponse
{
    [JsonPropertyName("ret")]
    public string Ret { get; set; } = string.Empty;

    [JsonPropertyName("task_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaskId { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UmengError? ErrorResponse { get; set; }
}

internal class UmengError
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("msg")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMsg { get; set; }
}
