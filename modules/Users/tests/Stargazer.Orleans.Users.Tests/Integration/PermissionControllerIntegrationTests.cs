using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class PermissionControllerIntegrationTests : IntegrationTestBase
{
    public PermissionControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPermissions_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>("api/permission");
        
        Assert.False(success);
    }

    [Fact]
    public async Task GetPermissions_WithAuth_ReturnsPermissions()
    {
        var account = $"perm_view_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var (success, permissions, _) = await GetAsync<object>("api/permission");

        Assert.True(success);
    }

    [Fact]
    public async Task CreatePermission_WithValidInput_ReturnsSuccess()
    {
        var account = $"perm_create_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var permCode = $"test.permission.{Guid.NewGuid():N}";
        var (success, permData, _) = await PostAsync<PermissionDataDto>("api/permission", new PermissionDataDto
        {
            Name = $"Test Permission {Guid.NewGuid():N}",
            Code = permCode,
            Description = "Test permission description",
            Category = "Test"
        });

        Assert.True(success);
        Assert.NotNull(permData);
        Assert.Equal(permCode, permData.Code);
    }

    [Fact]
    public async Task GetPermission_ById_ReturnsPermission()
    {
        var account = $"perm_get_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var permCode = $"get.permission.{Guid.NewGuid():N}";
        var (_, createdPerm, _) = await PostAsync<PermissionDataDto>("api/permission", new PermissionDataDto
        {
            Name = "Get Permission",
            Code = permCode,
            Description = "Get permission",
            Category = "Test"
        });

        var (success, permData, _) = await GetAsync<PermissionDataDto>($"api/permission/{createdPerm!.Id}");

        Assert.True(success);
        Assert.NotNull(permData);
        Assert.Equal(permCode, permData.Code);
    }

    [Fact]
    public async Task GetPermission_NotFound_ReturnsError()
    {
        var account = $"perm_notfound_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);
        var nonExistentId = Guid.NewGuid();

        var (success, _, errorCode) = await GetAsync<PermissionDataDto>($"api/permission/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("permission_not_found", errorCode);
    }

    [Fact]
    public async Task UpdatePermission_WithValidInput_ReturnsSuccess()
    {
        var account = $"perm_update_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var permCode = $"update.permission.{Guid.NewGuid():N}";
        var (_, createdPerm, _) = await PostAsync<PermissionDataDto>("api/permission", new PermissionDataDto
        {
            Name = "Original Permission",
            Code = permCode,
            Description = "Original description",
            Category = "Test"
        });

        var (success, updatedPerm, _) = await PutAsync<PermissionDataDto>($"api/permission/{createdPerm!.Id}", new PermissionDataDto
        {
            Name = "Updated Permission",
            Code = permCode,
            Description = "Updated description",
            Category = "Test"
        });

        Assert.True(success);
        Assert.NotNull(updatedPerm);
        Assert.Equal("Updated description", updatedPerm.Description);
    }

    [Fact]
    public async Task DeletePermission_ExistingPermission_ReturnsSuccess()
    {
        var account = $"perm_delete_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var permCode = $"delete.permission.{Guid.NewGuid():N}";
        var (_, createdPerm, _) = await PostAsync<PermissionDataDto>("api/permission", new PermissionDataDto
        {
            Name = "Delete Permission",
            Code = permCode,
            Description = "To be deleted",
            Category = "Test"
        });

        var (success, _, _) = await DeleteAsync<bool>($"api/permission/{createdPerm!.Id}");

        Assert.True(success);
    }

    [Fact]
    public async Task GetPermissionsByCategory_ReturnsPermissions()
    {
        var account = $"perm_category_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);

        var category = $"category_{Guid.NewGuid():N}";
        var permCode = $"category.permission.{Guid.NewGuid():N}";
        await PostAsync<PermissionDataDto>("api/permission", new PermissionDataDto
        {
            Name = "Category Permission",
            Code = permCode,
            Description = "Category test",
            Category = category
        });

        var (success, permissions, _) = await GetAsync<object>($"api/permission/category/{category}");

        Assert.True(success);
    }
}
