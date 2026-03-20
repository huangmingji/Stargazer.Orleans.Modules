namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Enums;

/// <summary>
/// 消息状态枚举
/// </summary>
public enum MessageStatusEnum
{
    Pending = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}
