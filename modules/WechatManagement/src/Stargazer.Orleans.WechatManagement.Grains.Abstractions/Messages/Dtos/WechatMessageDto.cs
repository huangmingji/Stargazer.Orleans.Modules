namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;

public class WechatMessageLogDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? Content { get; set; }
    public int Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SendTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public string? MsgId { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateWechatMessageInputDto
{
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? Content { get; set; }
}

public class SendTemplateMessageInputDto
{
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, TemplateMessageDataItem> Data { get; set; } = new();
}

public class TemplateMessageDataItem
{
    public string Value { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class SendCustomMessageInputDto
{
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MediaId { get; set; }
    public string? ThumbMediaId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? MusicUrl { get; set; }
    public string? HqMusicUrl { get; set; }
}

public class SendMassMessageInputDto
{
    public Guid AccountId { get; set; }
    public List<string> OpenIds { get; set; } = new();
    public Guid? TagId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MediaId { get; set; }
}

public class SendPassiveReplyInputDto
{
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
