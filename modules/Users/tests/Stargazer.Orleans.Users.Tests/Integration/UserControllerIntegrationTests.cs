using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class UserControllerIntegrationTests : IntegrationTestBase
{
    public UserControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<(Guid UserId, string Token)> CreateUserWithTokenAsync(string? roleName = null)
    {
        var account = $"user_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        if (roleName != null)
        {
            var (_, roleData, _) = await PostAsync<RoleDataDto>("api/role", new CreateOrUpdateRoleInputDto
            {
                Name = roleName,
                Description = "Test role"
            });

            if (roleData != null)
            {
                await PostAsync<object>($"api/user/{data!.User.Id}/roles", new List<Guid> { roleData.Id });
            }
        }

        return (data!.User.Id, data.AccessToken);
    }

    #region GetCurrentUser

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>("api/user/current");
        
        Assert.False(success);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUser()
    {
        var account = $"current_user_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        SetAuthToken(data!.AccessToken);
        var (success, userData, _) = await GetAsync<UserDataDto>("api/current-user");

        Assert.True(success);
        Assert.NotNull(userData);
        Assert.Equal(account, userData.Account);
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, userData, _) = await GetAsync<UserDataDto>($"api/user/{userId}");

        Assert.True(success);
        Assert.NotNull(userData);
        Assert.Equal(userId, userData.Id);
    }

    [Fact]
    public async Task GetUser_NotFound_ReturnsError()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);
        var nonExistentId = Guid.NewGuid();
        
        var (success, _, errorCode) = await GetAsync<UserDataDto>($"api/user/{nonExistentId}");
        
        Assert.False(success);
        Assert.Equal("user_not_found", errorCode);
    }

    #endregion

    #region GetUsers

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>("api/user");
        
        Assert.False(success);
    }

    [Fact]
    public async Task GetUsers_WithAuth_ReturnsPageResult()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, result, _) = await GetAsync<PageResult<UserDataDto>>("api/user");

        Assert.True(success);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task GetUsers_WithKeyword_ReturnsFilteredResults()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var account = $"filter_user_{Guid.NewGuid():N}";
        await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        var (success, result, _) = await GetAsync<PageResult<UserDataDto>>($"api/user?keyword={account}");

        Assert.True(success);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateUser

    [Fact]
    public async Task CreateUser_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await PostAsync<object>("api/user", new CreateOrUpdateUserInputDto
        {
            Account = "new_user",
            Password = "Test@123456",
            Email = "test@test.com"
        });
        
        Assert.False(success);
    }

    [Fact]
    public async Task CreateUser_WithValidInput_ReturnsSuccess()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var newAccount = $"create_user_{Guid.NewGuid():N}";
        // 随机生成电话号码 
        var phoneNumber = $"1{new Random().Next(100000000, 999999999)}";
        var (success, _, errorCode) = await PostAsync<object>("api/user", new CreateOrUpdateUserInputDto
        {
            Account = newAccount,
            Password = "Test@123456",
            Name = "Test User",
            Email = $"{Guid.NewGuid():N}@test.com",
            PhoneNumber = phoneNumber,
            Avatar = "https://example.com/avatar.jpg",
        });

        Assert.True(success);
    }

    #endregion

    #region UpdateUser

    [Fact]
    public async Task UpdateUser_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await PutAsync<object>($"api/user/{Guid.NewGuid()}", new CreateOrUpdateUserInputDto
        {
            Account = "updated_user",
            Password = "Test@123456"
        });
        
        Assert.False(success);
    }

    [Fact]
    public async Task UpdateUser_WithValidInput_ReturnsSuccess()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);


        var newAccount = $"update_user_{Guid.NewGuid():N}";
        // 随机生成电话号码 
        var phoneNumber = $"1{new Random().Next(100000000, 999999999)}";
        var (success, _, _) = await PutAsync<object>($"api/user/{userId}", new CreateOrUpdateUserInputDto
        {
            Account = newAccount,
            Password = "Test@123456",
            Name = "Updated Name",
            Email = $"{Guid.NewGuid()}@test.com",
            PhoneNumber = phoneNumber,
            Avatar = "https://example.com/avatar.jpg",
        });

        Assert.True(success);
    }

    #endregion

    #region DeleteUser

    [Fact]
    public async Task DeleteUser_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await DeleteAsync<object>($"api/user/{Guid.NewGuid()}");
        
        Assert.False(success);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsSuccess()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var newAccount = $"delete_user_{Guid.NewGuid():N}";
        var (_, registerData, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = newAccount,
            Password = "Test@123456"
        });

        var (success, _, _) = await DeleteAsync<bool>($"api/user/{registerData!.User.Id}");

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteUser_NotFound_ReturnsError()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, _, errorCode) = await DeleteAsync<bool>($"api/user/{Guid.NewGuid()}");

        Assert.False(success);
        Assert.Equal("user_not_found", errorCode);
    }

    #endregion

    #region AssignRoles

    [Fact]
    public async Task AssignRoles_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await PostAsync<object>($"api/user/{Guid.NewGuid()}/roles", new List<Guid>());
        
        Assert.False(success);
    }

    [Fact]
    public async Task AssignRoles_WithValidInput_ReturnsSuccess()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var roleName = $"assign_role_{Guid.NewGuid():N}";
        var (_, roleData, _) = await PostAsync<RoleDataDto>("api/role", new CreateOrUpdateRoleInputDto
        {
            Name = roleName,
            Description = "Role for assignment"
        });

        var (success, _, _) = await PostAsync<object>($"api/user/{userId}/roles", new List<Guid> { roleData!.Id });

        Assert.True(success);
    }

    [Fact]
    public async Task AssignRoles_UserNotFound_ReturnsError()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, _, errorCode) = await PostAsync<object>($"api/user/{Guid.NewGuid()}/roles", new List<Guid>());

        Assert.False(success);
        Assert.Equal("user_not_found", errorCode);
    }

    #endregion

    #region GetUserRoles

    [Fact]
    public async Task GetUserRoles_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>($"api/user/{Guid.NewGuid()}/roles");
        
        Assert.False(success);
    }

    [Fact]
    public async Task GetUserRoles_WithValidId_ReturnsRoles()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, roles, _) = await GetAsync<object>($"api/user/{userId}/roles");

        Assert.True(success);
    }

    #endregion

    #region GetUserPermissions

    [Fact]
    public async Task GetUserPermissions_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await GetAsync<object>($"api/user/{Guid.NewGuid()}/permissions");
        
        Assert.False(success);
    }

    [Fact]
    public async Task GetUserPermissions_WithValidId_ReturnsPermissions()
    {
        var (userId, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, permissions, _) = await GetAsync<object>($"api/user/{userId}/permissions");

        Assert.True(success);
    }

    #endregion

    #region UpdateUserStatus

    [Fact]
    public async Task UpdateUserStatus_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthToken();
        var (success, _, _) = await PostAsync<object>(
            $"api/user/{Guid.NewGuid()}/status", 
            new { isEnabled = false });
        
        Assert.False(success);
    }

    [Fact]
    public async Task UpdateUserStatus_Disable_ReturnsSuccess()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var newAccount = $"status_user_{Guid.NewGuid():N}";
        var (_, registerData, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = newAccount,
            Password = "Test@123456"
        });

        var (success, _, errorCode) = await PatchAsync<object>(
            $"api/user/{registerData!.User.Id}/status", 
            new UpdateUserStatusInputDto { IsEnabled = false });

        Assert.True(success);
    }

    [Fact]
    public async Task UpdateUserStatus_Enable_ReturnsSuccess()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var newAccount = $"status_user_{Guid.NewGuid():N}";
        var (_, registerData, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = newAccount,
            Password = "Test@123456"
        });

        var (success, _, errorCode) = await PatchAsync<object>(
            $"api/user/{registerData!.User.Id}/status", 
            new UpdateUserStatusInputDto { IsEnabled = true });

        Assert.True(success);
    }

    [Fact]
    public async Task UpdateUserStatus_UserNotFound_ReturnsError()
    {
        var (_, token) = await CreateUserWithTokenAsync();
        SetAuthToken(token);

        var (success, _, errorCode) = await PatchAsync<object>(
            $"api/user/{Guid.NewGuid()}/status", 
            new UpdateUserStatusInputDto { IsEnabled = false });

        Assert.False(success);
        Assert.Equal("user_not_found", errorCode);
    }

    #endregion
}
