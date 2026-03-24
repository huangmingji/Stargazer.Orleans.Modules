using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 批量发送消息输入参数
/// </summary>
[GenerateSerializer]
public class BatchSendMessageInputDto
{
    /// <summary>
    /// 消息通道
    /// </summary>
    [Id(0)]
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 接收者列表
    /// </summary>
    [Id(1)]
    public List<string> Receivers { get; set; } = new();

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
    /// 模板代码（可选）
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
    /// 发送者ID
    /// </summary>
    [Id(7)]
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    [Id(8)]
    public string? BusinessId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Id(9)]
    public string? BusinessType { get; set; }
}
