namespace Stargazer.Orleans.Users.Domain.Permissions;

public sealed class PermissionData : Entity<Guid>
{
    public string Name { get; set; } = "";
    
    public string Code { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public string Category { get; set; } = "";
    
    public PermissionType Type { get; set; } = PermissionType.Operation;
    
    public bool IsActive { get; set; } = true;
    
    public List<Stargazer.Orleans.Users.Domain.Roles.RoleData> Roles { get; set; } = new();
}

public enum PermissionType
{
    Operation,
    Menu,
    Button,
    Api
}
