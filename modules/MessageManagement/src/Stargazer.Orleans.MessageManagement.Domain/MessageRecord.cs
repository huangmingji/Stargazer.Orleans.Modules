using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Domain;

/// <summary>
/// 消息记录实体
/// 记录所有消息发送的详细信息
/// </summary>
public class MessageRecord : Entity<Guid>
{
    /// <summary>
    /// 消息通道：Email/SMS/Push
    /// </summary>
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 关联的模板ID（可选）
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 模板代码（用于直接发送）
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 接收者标识
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
    /// 模板变量（JSON格式）
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// 使用的Provider
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 消息状态
    /// </summary>
    public MessageStatus Status { get; set; } = MessageStatus.Pending;

    /// <summary>
    /// 外部追踪ID（如短信回执ID）
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 定时发送时间（可选）
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 关联业务ID（如订单ID）
    /// </summary>
    public string? BusinessId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public string? BusinessType { get; set; }
}
