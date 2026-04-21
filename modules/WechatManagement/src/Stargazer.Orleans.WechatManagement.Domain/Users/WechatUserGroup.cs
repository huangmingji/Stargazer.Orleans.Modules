namespace Stargazer.Orleans.WechatManagement.Domain.Users;

public class WechatUserGroup : Entity<Guid>
{
    public Guid AccountId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int WechatGroupId { get; set; }
    
    public int UserCount { get; set; }
    
    public virtual ICollection<WechatUser> Users { get; set; } = new List<WechatUser>();
    
    public virtual Accounts.WechatAccount? Account { get; set; }
}
