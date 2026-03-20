namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 消息记录输出DTO
/// </summary>
public class MessageRecordDto
{
    public Guid Id { get; set; }

    public string Channel { get; set; } = string.Empty;

    public Guid? TemplateId { get; set; }

    public string? TemplateCode { get; set; }

    public string Receiver { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string Content { get; set; } = string.Empty;

    public Dictionary<string, string>? Variables { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public Guid? SenderId { get; set; }

    public string? BusinessId { get; set; }

    public string? BusinessType { get; set; }

    public DateTime CreationTime { get; set; }
}
