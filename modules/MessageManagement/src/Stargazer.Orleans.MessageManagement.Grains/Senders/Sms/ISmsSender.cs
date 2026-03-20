namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

/// <summary>
/// 短信发送器接口
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// Provider名称
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送短信
    /// </summary>
    Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量发送短信
    /// </summary>
    Task<List<SmsSendResult>> BatchSendAsync(
        List<SmsSendRequest> requests,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送文本短信（部分Provider支持）
    /// </summary>
    Task<SmsSendResult> SendTextAsync(
        string phoneNumber,
        string content,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 短信发送结果
/// </summary>
public class SmsSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 短信发送请求
/// </summary>
public class SmsSendRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public Dictionary<string, string>? TemplateParams { get; set; }
}
