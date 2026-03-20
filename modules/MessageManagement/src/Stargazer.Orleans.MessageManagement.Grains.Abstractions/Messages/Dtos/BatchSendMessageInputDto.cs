using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Enums;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 批量发送消息输入参数
/// </summary>
public class BatchSendMessageInputDto
{
    /// <summary>
    /// 消息通道
    /// </summary>
    public MessageChannelEnum Channel { get; set; }

    /// <summary>
    /// 接收者列表
    /// </summary>
    public List<string> Receivers { get; set; } = new();

    /// <summary>
    /// 主题（邮件专用）
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（可选）
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
