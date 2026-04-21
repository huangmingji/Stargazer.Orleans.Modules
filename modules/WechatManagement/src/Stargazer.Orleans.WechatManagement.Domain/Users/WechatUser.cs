namespace Stargazer.Orleans.WechatManagement.Domain.Users;

public class WechatUser : Entity<Guid>
{
    public string OpenId { get; set; } = string.Empty;
    
    public string? UnionId { get; set; }
    
    public Guid AccountId { get; set; }
    
    public string Nickname { get; set; } = string.Empty;
    
    public int Sex { get; set; }
    
    public string? Province { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }
    
    public string? HeadimgUrl { get; set; }
    
    public DateTime? SubscribeTime { get; set; }
    
    public DateTime? UnSubscribeTime { get; set; }
    
    public string Remark { get; set; } = string.Empty;
    
    public int SubscribeStatus { get; set; }
    
    public Guid? GroupId { get; set; }
    
    public virtual WechatUserGroup? Group { get; set; }
    
    public virtual ICollection<WechatUserTag> Tags { get; set; } = new List<WechatUserTag>();
    
    public virtual Accounts.WechatAccount? Account { get; set; }
}
