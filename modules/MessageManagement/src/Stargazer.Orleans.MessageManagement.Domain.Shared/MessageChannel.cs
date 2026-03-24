namespace Stargazer.Orleans.MessageManagement.Domain.Shared;

/// <summary>
/// 消息发送通道枚举
/// </summary>
public enum MessageChannel
{
    /// <summary>
    /// 电子邮件
    /// </summary>
    Email = 1,

    /// <summary>
    /// 短信
    /// </summary>
    Sms = 2,

    /// <summary>
    /// App推送通知
    /// </summary>
    Push = 3
}
