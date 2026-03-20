namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Email;

/// <summary>
/// Email发送器接口
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Provider名称
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送邮件
    /// </summary>
    Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送邮件（带发件人）
    /// </summary>
    Task<EmailSendResult> SendAsync(
        string from,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量发送邮件
    /// </summary>
    Task<List<EmailSendResult>> BatchSendAsync(
        List<EmailSendRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Email发送结果
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Email发送请求
/// </summary>
public class EmailSendRequest
{
    public string To { get; set; } = string.Empty;
    public string? From { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
