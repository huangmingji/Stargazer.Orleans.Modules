namespace Stargazer.Orleans.MessageManagement.Domain.Shared;

/// <summary>
/// 消息发送状态枚举
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// 待发送
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 发送中
    /// </summary>
    Sending = 1,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 2,

    /// <summary>
    /// 已送达
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 4,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 5
}
