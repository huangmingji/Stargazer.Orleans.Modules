namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

/// <summary>
/// 推送发送器接口
/// </summary>
public interface IPushSender
{
    /// <summary>
    /// Provider名称
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送推送通知
    /// </summary>
    Task<PushSendResult> SendAsync(
        PushRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量发送推送
    /// </summary>
    Task<List<PushSendResult>> BatchSendAsync(
        List<PushRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 推送发送结果
/// </summary>
public class PushSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 推送请求
/// </summary>
public class PushRequest
{
    /// <summary>
    /// 目标类型：registration_id / alias / tag / all
    /// </summary>
    public string TargetType { get; set; } = "all";

    /// <summary>
    /// 目标值列表
    /// </summary>
    public List<string> Targets { get; set; } = new();

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 额外参数
    /// </summary>
    public Dictionary<string, string>? Extras { get; set; }

    /// <summary>
    /// iOS环境：true=生产环境，false=开发环境
    /// </summary>
    public bool ApnsProduction { get; set; } = true;
}
