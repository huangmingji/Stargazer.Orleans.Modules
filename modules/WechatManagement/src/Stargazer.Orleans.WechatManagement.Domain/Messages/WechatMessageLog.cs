namespace Stargazer.Orleans.WechatManagement.Domain.Messages;

public class WechatMessageLog : Entity<Guid>
{
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
    
    public string? ExtraData { get; set; }
    
    public virtual Accounts.WechatAccount? Account { get; set; }
}

public static class WechatMessageStatus
{
    public const int Pending = 0;
    public const int Sending = 1;
    public const int Success = 2;
    public const int Failed = 3;
    public const int Cancelled = 4;
}

public static class WechatMessageType
{
    public const string Template = "template";
    public const string Custom = "custom";
    public const string Mass = "mass";
    public const string PassiveReply = "passive_reply";
}
