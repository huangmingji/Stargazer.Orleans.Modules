namespace Stargazer.Orleans.Users.Domain.UserRoles;

public sealed class UserRoleData : Entity<Guid>
{
    public Guid UserId { get; set; }
    
    public Guid RoleId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpireTime { get; set; }
}
