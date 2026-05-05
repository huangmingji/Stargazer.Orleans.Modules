using System.Text.Json.Serialization;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

/// <summary>
/// 消息记录输出DTO
/// </summary>
[GenerateSerializer]
public class MessageRecordDto
{
    [Id(0)] [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)] [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [Id(2)] [JsonPropertyName("template_id")]
    public Guid? TemplateId { get; set; }

    [Id(3)] [JsonPropertyName("template_code")]
    public string? TemplateCode { get; set; }

    [Id(4)] [JsonPropertyName("receiver")]
    public string Receiver { get; set; } = string.Empty;

    [Id(5)] [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [Id(6)] [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [Id(7)] [JsonPropertyName("variables")]
    public Dictionary<string, string>? Variables { get; set; }

    [Id(8)] [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [Id(9)] [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [Id(10)] [JsonPropertyName("external_id")]
    public string? ExternalId { get; set; }

    [Id(11)] [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; set; }

    [Id(12)] [JsonPropertyName("retry_count")]
    public int RetryCount { get; set; }

    [Id(13)] [JsonPropertyName("sent_at")]
    public DateTime? SentAt { get; set; }

    [Id(14)] [JsonPropertyName("delivered_at")]
    public DateTime? DeliveredAt { get; set; }

    [Id(15)] [JsonPropertyName("scheduled_at")]
    public DateTime? ScheduledAt { get; set; }

    [Id(16)] [JsonPropertyName("sender_id")]
    public Guid? SenderId { get; set; }

    [Id(17)] [JsonPropertyName("business_id")]
    public string? BusinessId { get; set; }

    [Id(18)] [JsonPropertyName("business_type")]
    public string? BusinessType { get; set; }

    [Id(19)] [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}
