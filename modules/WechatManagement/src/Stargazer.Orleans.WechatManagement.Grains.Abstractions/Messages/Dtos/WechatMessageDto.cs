using System.Text.Json.Serialization;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;

public class WechatMessageLogDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("send_time")]
    public DateTime? SendTime { get; set; }

    [JsonPropertyName("complete_time")]
    public DateTime? CompleteTime { get; set; }

    [JsonPropertyName("msg_id")]
    public string? MsgId { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }

    [JsonPropertyName("last_modify_time")]
    public DateTime? LastModifyTime { get; set; }
}

public class CreateWechatMessageInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

public class SendTemplateMessageInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("template_id")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, TemplateMessageDataItem> Data { get; set; } = new();
}

public class TemplateMessageDataItem
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string? Color { get; set; }
}

public class SendCustomMessageInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("media_id")]
    public string? MediaId { get; set; }

    [JsonPropertyName("thumb_media_id")]
    public string? ThumbMediaId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("music_url")]
    public string? MusicUrl { get; set; }

    [JsonPropertyName("hq_music_url")]
    public string? HqMusicUrl { get; set; }
}

public class SendMassMessageInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_ids")]
    public List<string> OpenIds { get; set; } = new();

    [JsonPropertyName("tag_id")]
    public Guid? TagId { get; set; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("media_id")]
    public string? MediaId { get; set; }
}

public class SendPassiveReplyInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
