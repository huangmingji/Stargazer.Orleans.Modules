namespace Stargazer.Orleans.WechatManagement.Domain.Users;

public class WechatUserBinding : Entity<Guid>
{
    public Guid WechatUserId { get; set; }

    public Guid LocalUserId { get; set; }

    public Guid AccountId { get; set; }

    public string OpenId { get; set; } = string.Empty;

    public DateTime BindingTime { get; set; }

    public bool IsActive { get; set; } = true;
}

public static class WechatBindingStatus
{
    public const int Pending = 0;
    public const int Confirmed = 1;
    public const int Cancelled = 2;
}