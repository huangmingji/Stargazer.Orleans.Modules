using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class RoleControllerIntegrationTests : IntegrationTestBase
{
    public RoleControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRoles_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>("users/api/role");
        
        Assert.False(success);
    }

    [Fact]
    public async Task CreateRole_WithValidInput_ReturnsSuccess()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);

        var roleName = $"TestRole_{Guid.NewGuid():N}";
        var (success, roleData, _) = await PostAsync<RoleDataDto>("users/api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Test role description"
        });

        Assert.True(success);
        Assert.NotNull(roleData);
        Assert.Equal(roleName, roleData.Name);
    }

    [Fact]
    public async Task GetRole_ById_ReturnsRole()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);

        var roleName = $"GetRole_{Guid.NewGuid():N}";
        var (_, createdRole, _) = await PostAsync<RoleDataDto>("users/api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Test role"
        });

        var (success, roleData, _) = await GetAsync<RoleDataDto>($"users/api/role/{createdRole!.Id}");

        Assert.True(success);
        Assert.NotNull(roleData);
        Assert.Equal(roleName, roleData.Name);
    }

    [Fact]
    public async Task GetRole_NotFound_ReturnsError()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);
        var nonExistentId = Guid.NewGuid();

        var (success, _, errorCode) = await GetAsync<RoleDataDto>($"users/api/role/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("role_not_found", errorCode);
    }

    [Fact]
    public async Task UpdateRole_WithValidInput_ReturnsSuccess()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);

        var roleName = $"UpdateRole_{Guid.NewGuid():N}";
        var (_, createdRole, _) = await PostAsync<RoleDataDto>("users/api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Original description"
        });

        var (success, updatedRole, _) = await PutAsync<RoleDataDto>($"users/api/role/{createdRole!.Id}", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Updated description"
        });

        Assert.True(success);
        Assert.NotNull(updatedRole);
        Assert.Equal("Updated description", updatedRole.Description);
    }

    [Fact]
    public async Task DeleteRole_ExistingRole_ReturnsSuccess()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);

        var roleName = $"DeleteRole_{Guid.NewGuid():N}";
        var (_, createdRole, _) = await PostAsync<RoleDataDto>("users/api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "To be deleted"
        });

        var (success, _, _) = await DeleteAsync<bool>($"users/api/role/{createdRole!.Id}");

        Assert.True(success);
    }

    [Fact]
    public async Task GetRolePermissions_ReturnsPermissions()
    {
        var (_, data, _) = await PostAsync<TokenResponseDto>("users/api/account/login", new VerifyPasswordInputDto()
        {
            Account = "admin",
            Password = "Admin@123456"
        });

        SetAuthToken(data!.AccessToken);

        var roleName = $"PermsRole_{Guid.NewGuid():N}";
        var (_, createdRole, _) = await PostAsync<RoleDataDto>("users/api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Role with permissions"
        });

        var (success, permissions, _) = await GetAsync<object>($"users/api/role/{createdRole!.Id}/permissions");

        Assert.True(success);
    }
}
