using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 发送消息输入参数
/// </summary>
[GenerateSerializer]
public class SendMessageInputDto
{
    /// <summary>
    /// 消息通道
    /// </summary>
    [Id(0)]
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 接收者（邮箱/手机号/设备Token）
    /// </summary>
    [Id(1)]
    public string Receiver { get; set; } = string.Empty;

    /// <summary>
    /// 主题（邮件专用）
    /// </summary>
    [Id(2)]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [Id(3)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（可选，用于模板发送）
    /// </summary>
    [Id(4)]
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 模板变量
    /// </summary>
    [Id(5)]
    public Dictionary<string, string>? Variables { get; set; }

    /// <summary>
    /// 指定Provider（可选）
    /// </summary>
    [Id(6)]
    public string? Provider { get; set; }

    /// <summary>
    /// 定时发送时间（可选）
    /// </summary>
    [Id(7)]
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    [Id(8)]
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    [Id(9)]
    public string? BusinessId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Id(10)]
    public string? BusinessType { get; set; }
}
