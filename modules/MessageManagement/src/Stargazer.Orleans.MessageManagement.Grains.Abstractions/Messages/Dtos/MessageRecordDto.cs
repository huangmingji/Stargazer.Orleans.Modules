namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 消息记录输出DTO
/// </summary>
[GenerateSerializer]
public class MessageRecordDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Channel { get; set; } = string.Empty;

    [Id(2)]
    public Guid? TemplateId { get; set; }

    [Id(3)]
    public string? TemplateCode { get; set; }

    [Id(4)]
    public string Receiver { get; set; } = string.Empty;

    [Id(5)]
    public string? Subject { get; set; }

    [Id(6)]
    public string Content { get; set; } = string.Empty;

    [Id(7)]
    public Dictionary<string, string>? Variables { get; set; }

    [Id(8)]
    public string Provider { get; set; } = string.Empty;

    [Id(9)]
    public string Status { get; set; } = string.Empty;

    [Id(10)]
    public string? ExternalId { get; set; }

    [Id(11)]
    public string? FailureReason { get; set; }

    [Id(12)]
    public int RetryCount { get; set; }

    [Id(13)]
    public DateTime? SentAt { get; set; }

    [Id(14)]
    public DateTime? DeliveredAt { get; set; }

    [Id(15)]
    public DateTime? ScheduledAt { get; set; }

    [Id(16)]
    public Guid? SenderId { get; set; }

    [Id(17)]
    public string? BusinessId { get; set; }

    [Id(18)]
    public string? BusinessType { get; set; }

    [Id(19)]
    public DateTime CreationTime { get; set; }
}
