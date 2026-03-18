namespace Stargazer.Orleans.Users.Domain.Roles;

public sealed class RoleData : Entity<Guid>
{
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public bool IsDefault { get; set; }
    
    public int Priority { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<Permissions.PermissionData> Permissions { get; set; } = new();
    
    public List<UserRoles.UserRoleData> UserRoles { get; set; } = new();
}
