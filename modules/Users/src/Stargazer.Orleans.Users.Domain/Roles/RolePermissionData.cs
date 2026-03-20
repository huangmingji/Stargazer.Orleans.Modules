using Stargazer.Orleans.Users.Domain.Permissions;

namespace Stargazer.Orleans.Users.Domain.Roles;

public class RolePermissionData : Entity<Guid>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public RoleData? Role { get; set; }
    public PermissionData? Permission { get; set; }
}
