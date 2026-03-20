using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Enums;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 发送消息输入参数
/// </summary>
public class SendMessageInputDto
{
    /// <summary>
    /// 消息通道
    /// </summary>
    public MessageChannelEnum Channel { get; set; }

    /// <summary>
    /// 接收者（邮箱/手机号/设备Token）
    /// </summary>
    public string Receiver { get; set; } = string.Empty;

    /// <summary>
    /// 主题（邮件专用）
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（可选，用于模板发送）
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 模板变量
    /// </summary>
    public Dictionary<string, string>? Variables { get; set; }

    /// <summary>
    /// 指定Provider（可选）
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 定时发送时间（可选）
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    public string? BusinessId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public string? BusinessType { get; set; }
}
