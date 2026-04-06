using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class UserControllerIntegrationTests : IntegrationTestBase
{
    public UserControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

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
        var (success, userData, _) = await GetAsync<UserDataDto>("api/user/current");

        Assert.True(success);
        Assert.NotNull(userData);
        Assert.Equal(account, userData.Account);
    }

    [Fact]
    public async Task GetUser_NotFound_ReturnsError()
    {
        ClearAuthToken();
        var nonExistentId = Guid.NewGuid();
        
        var (success, _, errorCode) = await GetAsync<UserDataDto>($"api/user/{nonExistentId}");
        
        Assert.False(success);
        Assert.Equal("user_not_found", errorCode);
    }

    [Fact]
    public async Task CheckAccountExists_ExistingAccount_ReturnsTrue()
    {
        var account = $"check_{Guid.NewGuid():N}";
        
        await PostAsync<object>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        var (success, data, _) = await GetAsync<bool>($"api/user/check/account/{account}");

        Assert.True(success);
        Assert.True(data);
    }

    [Fact]
    public async Task CheckAccountExists_NonExistingAccount_ReturnsFalse()
    {
        var (success, data, _) = await GetAsync<bool>($"api/user/check/account/non_existing_account_{Guid.NewGuid():N}");

        Assert.True(success);
        Assert.False(data);
    }

    [Fact]
    public async Task CheckEmailExists_NonExistingEmail_ReturnsFalse()
    {
        var (success, data, _) = await GetAsync<bool>($"api/user/check/email/nonexisting_{Guid.NewGuid():N}@test.com");

        Assert.True(success);
        Assert.False(data);
    }

    [Fact]
    public async Task HasPermission_WithValidRequest_ReturnsResult()
    {
        var account = $"perm_user_{Guid.NewGuid():N}";
        var (_, data, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        var (success, permData, _) = await PostAsync<bool>("api/user/has-permission", new
        {
            UserId = data!.User.Id,
            Permission = "users.view"
        });

        Assert.True(success);
    }
}
