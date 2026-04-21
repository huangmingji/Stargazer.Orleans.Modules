namespace Stargazer.Orleans.WechatManagement.Domain.Accounts;

public class WechatAccount : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    
    public string AppId { get; set; } = string.Empty;
    
    public string AppSecret { get; set; } = string.Empty;
    
    public string Token { get; set; } = string.Empty;
    
    public string EncodingAESKey { get; set; } = string.Empty;
    
    public bool IsDefault { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? AccessToken { get; set; }
    
    public DateTime? AccessTokenExpiry { get; set; }
    
    public virtual ICollection<Users.WechatUser> Users { get; set; } = new List<Users.WechatUser>();
    
    public virtual ICollection<Users.WechatUserGroup> UserGroups { get; set; } = new List<Users.WechatUserGroup>();
    
    public virtual ICollection<Users.WechatUserTag> UserTags { get; set; } = new List<Users.WechatUserTag>();
}
