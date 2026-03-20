using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Permissions;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Domain;

public class UserDataTests
{
    [Fact]
    public void NewUserData_HasDefaultValues()
    {
        var user = new UserData
        {
            Id = Guid.NewGuid(),
            Account = "testuser"
        };

        Assert.Equal("testuser", user.Account);
        Assert.True(user.IsActive);
        Assert.Empty(user.UserRoles);
    }

    [Fact]
    public void UserData_CanSetAllProperties()
    {
        var userId = Guid.NewGuid();
        var user = new UserData
        {
            Id = userId,
            Account = "testuser",
            Password = "hashed_password",
            SecretKey = "secret_key",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Avatar = "avatar.png",
            IsActive = true
        };

        Assert.Equal(userId, user.Id);
        Assert.Equal("testuser", user.Account);
        Assert.Equal("hashed_password", user.Password);
        Assert.Equal("secret_key", user.SecretKey);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("1234567890", user.PhoneNumber);
        Assert.Equal("avatar.png", user.Avatar);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void UserData_CanAddUserRoles()
    {
        var userId = Guid.NewGuid();
        var user = new UserData
        {
            Id = userId,
            Account = "testuser",
            UserRoles = new List<UserRoleData>
            {
                new() { Id = Guid.NewGuid(), RoleId = Guid.NewGuid(), UserId = userId }
            }
        };

        Assert.Single(user.UserRoles);
        Assert.Equal(userId, user.UserRoles[0].UserId);
    }

    [Fact]
    public void UserData_AuditFieldsAreSet()
    {
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var user = new UserData
        {
            Id = Guid.NewGuid(),
            Account = "testuser",
            CreatorId = creatorId,
            CreationTime = now
        };

        Assert.Equal(creatorId, user.CreatorId);
        Assert.Equal(now, user.CreationTime);
    }
}

public class RoleDataTests
{
    [Fact]
    public void NewRoleData_HasDefaultValues()
    {
        var role = new RoleData
        {
            Id = Guid.NewGuid(),
            Name = "TestRole"
        };

        Assert.Equal("TestRole", role.Name);
        Assert.Empty(role.RolePermissions);
        Assert.Empty(role.UserRoles);
        Assert.True(role.IsActive);
        Assert.False(role.IsDefault);
    }

    [Fact]
    public void RoleData_CanSetAllProperties()
    {
        var roleId = Guid.NewGuid();
        var role = new RoleData
        {
            Id = roleId,
            Name = "Admin",
            Description = "Administrator role",
            IsActive = true,
            IsDefault = false,
            Priority = 1
        };

        Assert.Equal(roleId, role.Id);
        Assert.Equal("Admin", role.Name);
        Assert.Equal("Administrator role", role.Description);
        Assert.True(role.IsActive);
        Assert.False(role.IsDefault);
        Assert.Equal(1, role.Priority);
    }

    [Fact]
    public void RoleData_CanAddRolePermissions()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var role = new RoleData
        {
            Id = roleId,
            Name = "Admin",
            RolePermissions = new List<RolePermissionData>
            {
                new() { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = permissionId }
            }
        };

        Assert.Single(role.RolePermissions);
        Assert.Equal(roleId, role.RolePermissions[0].RoleId);
        Assert.Equal(permissionId, role.RolePermissions[0].PermissionId);
    }

    [Fact]
    public void RoleData_AuditFieldsAreSet()
    {
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var role = new RoleData
        {
            Id = Guid.NewGuid(),
            Name = "TestRole",
            CreatorId = creatorId,
            CreationTime = now
        };

        Assert.Equal(creatorId, role.CreatorId);
        Assert.Equal(now, role.CreationTime);
    }
}

public class PermissionDataTests
{
    [Fact]
    public void NewPermissionData_HasDefaultValues()
    {
        var permission = new PermissionData
        {
            Id = Guid.NewGuid(),
            Name = "user.create",
            Description = "Create users"
        };

        Assert.Equal("user.create", permission.Name);
        Assert.Equal("Create users", permission.Description);
        Assert.True(permission.IsActive);
        Assert.Equal(PermissionType.Operation, permission.Type);
        Assert.Empty(permission.RolePermissions);
    }

    [Fact]
    public void PermissionData_CanSetAllProperties()
    {
        var permissionId = Guid.NewGuid();
        var permission = new PermissionData
        {
            Id = permissionId,
            Name = "role.view",
            Code = "role:view",
            Description = "View roles",
            Category = "Roles",
            Type = PermissionType.Operation,
            IsActive = false
        };

        Assert.Equal(permissionId, permission.Id);
        Assert.Equal("role.view", permission.Name);
        Assert.Equal("role:view", permission.Code);
        Assert.Equal("View roles", permission.Description);
        Assert.Equal("Roles", permission.Category);
        Assert.Equal(PermissionType.Operation, permission.Type);
        Assert.False(permission.IsActive);
    }

    [Theory]
    [InlineData(PermissionType.Operation)]
    [InlineData(PermissionType.Menu)]
    [InlineData(PermissionType.Button)]
    [InlineData(PermissionType.Api)]
    public void PermissionData_CanSetAllPermissionTypes(PermissionType type)
    {
        var permission = new PermissionData
        {
            Id = Guid.NewGuid(),
            Name = "test",
            Type = type
        };

        Assert.Equal(type, permission.Type);
    }

    [Fact]
    public void PermissionData_CanAddRolePermissions()
    {
        var permissionId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permission = new PermissionData
        {
            Id = permissionId,
            Name = "user.create",
            RolePermissions = new List<RolePermissionData>
            {
                new() { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = permissionId }
            }
        };

        Assert.Single(permission.RolePermissions);
        Assert.Equal(permissionId, permission.RolePermissions[0].PermissionId);
        Assert.Equal(roleId, permission.RolePermissions[0].RoleId);
    }

    [Fact]
    public void PermissionData_AuditFieldsAreSet()
    {
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var permission = new PermissionData
        {
            Id = Guid.NewGuid(),
            Name = "test",
            CreatorId = creatorId,
            CreationTime = now
        };

        Assert.Equal(creatorId, permission.CreatorId);
        Assert.Equal(now, permission.CreationTime);
    }
}

public class UserRoleDataTests
{
    [Fact]
    public void NewUserRoleData_HasDefaultValues()
    {
        var userRole = new UserRoleData
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid()
        };

        Assert.True(userRole.IsActive);
        Assert.Null(userRole.ExpireTime);
    }

    [Fact]
    public void UserRoleData_CanSetAllProperties()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var expireTime = DateTime.UtcNow.AddDays(30);
        var userRole = new UserRoleData
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            ExpireTime = expireTime
        };

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
        Assert.True(userRole.IsActive);
        Assert.Equal(expireTime, userRole.ExpireTime);
    }

    [Fact]
    public void UserRoleData_CanBeExpired()
    {
        var userRole = new UserRoleData
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            IsActive = false,
            ExpireTime = DateTime.UtcNow.AddDays(-1)
        };

        Assert.False(userRole.IsActive);
        Assert.True(userRole.ExpireTime < DateTime.UtcNow);
    }

    [Fact]
    public void UserRoleData_AuditFieldsAreSet()
    {
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var userRole = new UserRoleData
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            CreatorId = creatorId,
            CreationTime = now
        };

        Assert.Equal(creatorId, userRole.CreatorId);
        Assert.Equal(now, userRole.CreationTime);
    }
}

public class RolePermissionDataTests
{
    [Fact]
    public void NewRolePermissionData_CanSetProperties()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var rolePermission = new RolePermissionData
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId
        };

        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
        Assert.Null(rolePermission.Role);
        Assert.Null(rolePermission.Permission);
    }

    [Fact]
    public void RolePermissionData_CanSetNavigationProperties()
    {
        var role = new RoleData { Id = Guid.NewGuid(), Name = "Admin" };
        var permission = new PermissionData { Id = Guid.NewGuid(), Name = "user.create" };
        var rolePermission = new RolePermissionData
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = permission.Id,
            Role = role,
            Permission = permission
        };

        Assert.NotNull(rolePermission.Role);
        Assert.NotNull(rolePermission.Permission);
        Assert.Equal("Admin", rolePermission.Role.Name);
        Assert.Equal("user.create", rolePermission.Permission.Name);
    }

    [Fact]
    public void RolePermissionData_AuditFieldsAreSet()
    {
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var rolePermission = new RolePermissionData
        {
            Id = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            CreatorId = creatorId,
            CreationTime = now
        };

        Assert.Equal(creatorId, rolePermission.CreatorId);
        Assert.Equal(now, rolePermission.CreationTime);
    }
}
