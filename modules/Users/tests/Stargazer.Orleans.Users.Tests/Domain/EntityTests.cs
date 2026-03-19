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
    public void UserData_CanAddRoles()
    {
        var user = new UserData
        {
            Id = Guid.NewGuid(),
            Account = "testuser",
            UserRoles = new List<UserRoleData>
            {
                new() { RoleId = Guid.NewGuid(), UserId = Guid.NewGuid() }
            }
        };

        Assert.Single(user.UserRoles);
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
        Assert.Empty(role.Permissions);
        Assert.Empty(role.UserRoles);
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
    public void RoleData_CanAddPermissions()
    {
        var role = new RoleData
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Permissions = new List<PermissionData>
            {
                new() { Id = Guid.NewGuid(), Name = "user.create" }
            }
        };

        Assert.Single(role.Permissions);
        Assert.Equal("user.create", role.Permissions[0].Name);
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
        Assert.Empty(permission.Roles);
    }

    [Fact]
    public void PermissionData_CanSetAllProperties()
    {
        var permissionId = Guid.NewGuid();
        var permission = new PermissionData
        {
            Id = permissionId,
            Name = "role.view",
            Description = "View roles",
            IsActive = false
        };

        Assert.Equal(permissionId, permission.Id);
        Assert.Equal("role.view", permission.Name);
        Assert.Equal("View roles", permission.Description);
        Assert.False(permission.IsActive);
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

        Assert.NotEqual(Guid.Empty, userRole.Id);
        Assert.NotEqual(Guid.Empty, userRole.UserId);
        Assert.NotEqual(Guid.Empty, userRole.RoleId);
    }

    [Fact]
    public void UserRoleData_CanSetAllProperties()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRole = new UserRoleData
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId
        };

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
    }
}
