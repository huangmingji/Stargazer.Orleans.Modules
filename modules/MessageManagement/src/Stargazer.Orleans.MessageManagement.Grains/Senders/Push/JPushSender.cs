using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

public class JPushSender : IPushSender
{
    private readonly JPushSettings _settings;
    private readonly ILogger<JPushSender> _logger;
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://api.jpush.cn";

    public string ProviderName => "jpush";

    public JPushSender(JPushSettings settings, ILogger<JPushSender> logger, HttpClient httpClient)
    {
        _settings = settings;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<PushSendResult> SendAsync(PushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var pushPayload = BuildPushPayload(request);
            var response = await ExecutePushAsync(pushPayload, cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "JPush message sent successfully. MsgId: {MsgId}, TargetType: {TargetType}",
                    response.MsgId,
                    request.TargetType);
                return new PushSendResult { Success = true, MessageId = response.MsgId };
            }
            else
            {
                _logger.LogWarning(
                    "JPush message send failed. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    response.Error?.Code,
                    response.Error?.Message);
                return new PushSendResult
                {
                    Success = false,
                    ErrorMessage = $"{response.Error?.Code}: {response.Error?.Message}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JPush send exception. TargetType: {TargetType}, Target: {Target}",
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

    private JPushPushPayload BuildPushPayload(PushRequest request)
    {
        var audience = request.TargetType.ToLowerInvariant() switch
        {
            "all" => new JPushAudience { All = "all" },
            "registration_id" => new JPushAudience { RegistrationId = request.Targets },
            "alias" => new JPushAudience { Alias = request.Targets },
            "tag" => new JPushAudience { Tag = request.Targets },
            _ => new JPushAudience { All = "all" }
        };

        var payload = new JPushPushPayload
        {
            Platform = "all",
            Audience = audience,
            Notification = new JPushNotification
            {
                Alert = request.Content,
                Android = new JPushAndroidNotification
                {
                    Alert = request.Content,
                    Title = request.Title,
                    Extras = request.Extras
                },
                Ios = new JPushIosNotification
                {
                    Alert = new JPushIosAlert
                    {
                        Title = request.Title,
                        Body = request.Content
                    },
                    Extras = request.Extras,
                    ApnsProduction = request.ApnsProduction
                }
            }
        };

        if (!string.IsNullOrEmpty(request.Content))
        {
            payload.Message = new JPushMessage
            {
                MsgContent = request.Content,
                Title = request.Title,
                Extras = request.Extras
            };
        }

        return payload;
    }

    private async Task<JPushResponse> ExecutePushAsync(JPushPushPayload payload, CancellationToken cancellationToken)
    {
        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.AppKey}:{_settings.MasterSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/v3/push");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("JPush API response: {Response}", content);

        if (!response.IsSuccessStatusCode)
        {
            return new JPushResponse
            {
                IsSuccess = false,
                Error = new JPushError
                {
                    Code = (int)response.StatusCode,
                    Message = content
                }
            };
        }

        try
        {
            var result = JsonSerializer.Deserialize<JPushResponse>(content, JsonOptions);
            if (result != null && !string.IsNullOrEmpty(result.MsgId))
            {
                result.IsSuccess = true;
            }
            return result ?? new JPushResponse { IsSuccess = false, Error = new JPushError { Code = -1, Message = "Parse response failed" } };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JPush response: {Content}", content);
            return new JPushResponse
            {
                IsSuccess = false,
                Error = new JPushError { Code = -1, Message = $"Parse error: {ex.Message}" }
            };
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}

internal class JPushPushPayload
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "all";

    [JsonPropertyName("audience")]
    public JPushAudience Audience { get; set; } = new();

    [JsonPropertyName("notification")]
    public JPushNotification? Notification { get; set; }

    [JsonPropertyName("message")]
    public JPushMessage? Message { get; set; }

    [JsonPropertyName("options")]
    public JPushOptions? Options { get; set; }
}

internal class JPushAudience
{
    [JsonPropertyName("all")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? All { get; set; }

    [JsonPropertyName("registration_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? RegistrationId { get; set; }

    [JsonPropertyName("alias")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Alias { get; set; }

    [JsonPropertyName("tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Tag { get; set; }
}

internal class JPushNotification
{
    [JsonPropertyName("alert")]
    public string Alert { get; set; } = string.Empty;

    [JsonPropertyName("android")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JPushAndroidNotification? Android { get; set; }

    [JsonPropertyName("ios")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JPushIosNotification? Ios { get; set; }
}

internal class JPushAndroidNotification
{
    [JsonPropertyName("alert")]
    public string Alert { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("extras")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Extras { get; set; }
}

internal class JPushIosNotification
{
    [JsonPropertyName("alert")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JPushIosAlert? Alert { get; set; }

    [JsonPropertyName("sound")]
    public string Sound { get; set; } = "default";

    [JsonPropertyName("badge")]
    public string Badge { get; set; } = "+1";

    [JsonPropertyName("content-available")]
    public int ContentAvailable { get; set; } = 0;

    [JsonPropertyName("mutable-content")]
    public int MutableContent { get; set; } = 0;

    [JsonPropertyName("extras")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Extras { get; set; }

    [JsonPropertyName("apns_production")]
    public bool ApnsProduction { get; set; } = true;
}

internal class JPushIosAlert
{
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body { get; set; }

    [JsonPropertyName("subtitle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subtitle { get; set; }
}

internal class JPushMessage
{
    [JsonPropertyName("msg_content")]
    public string MsgContent { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "text";

    [JsonPropertyName("extras")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Extras { get; set; }
}

internal class JPushOptions
{
    [JsonPropertyName("apns_production")]
    public bool ApnsProduction { get; set; } = true;

    [JsonPropertyName("time_to_live")]
    public int TimeToLive { get; set; } = 86400;
}

internal class JPushResponse
{
    [JsonPropertyName("msg_id")]
    public string? MsgId { get; set; }

    [JsonPropertyName("sendno")]
    public string? SendNo { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JPushError? Error { get; set; }

    [JsonIgnore]
    public bool IsSuccess { get; set; }
}

internal class JPushError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
