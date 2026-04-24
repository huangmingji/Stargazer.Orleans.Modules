using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class AccountControllerIntegrationTests : IntegrationTestBase
{
    public AccountControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidInput_ReturnsSuccess()
    {
        var input = new RegisterAccountInputDto
        {
            Account = $"test_user_{Guid.NewGuid():N}",
            Password = "Test@123456"
        };

        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/register", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.NotNull(data.AccessToken);
        Assert.NotNull(data.RefreshToken);
        Assert.NotNull(data.User);
        Assert.Equal(input.Account, data.User.Account);
    }

    [Fact]
    public async Task Register_WithDuplicateAccount_ReturnsError()
    {
        var account = $"duplicate_{Guid.NewGuid():N}";
        var input = new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        };

        await PostAsync<TokenResponseDto>("api/account/register", input);

        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/register", input);

        Assert.False(success);
        Assert.Equal("account_exists", errorCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var account = $"login_test_{Guid.NewGuid():N}";
        var password = "Test@123456";
        
        await PostAsync<object>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = password
        });

        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/login", new VerifyPasswordInputDto
        {
            Account = account,
            Password = password
        });

        Assert.True(success);
        Assert.NotNull(data);
        Assert.NotNull(data.AccessToken);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsError()
    {
        var account = $"login_fail_{Guid.NewGuid():N}";
        
        await PostAsync<object>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = "Test@123456"
        });

        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/login", new VerifyPasswordInputDto
        {
            Account = account,
            Password = "WrongPassword"
        });

        Assert.False(success);
        Assert.Equal("account_password_incorrect", errorCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var account = $"refresh_test_{Guid.NewGuid():N}";
        var password = "Test@123456";
        
        var (regSuccess, registerData, _) = await PostAsync<TokenResponseDto>("api/account/register", new RegisterAccountInputDto
        {
            Account = account,
            Password = password
        });
        
        Assert.True(regSuccess);

        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/refresh", new RefreshTokenInputDto
        {
            RefreshToken = registerData!.RefreshToken
        });

        Assert.True(success);
        Assert.NotNull(data);
        Assert.NotNull(data.AccessToken);
        Assert.NotNull(data.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsError()
    {
        var (success, data, errorCode) = await PostAsync<TokenResponseDto>("api/account/refresh", new RefreshTokenInputDto
        {
            RefreshToken = "invalid_token_string"
        });

        Assert.False(success);
        Assert.Equal("invalid_refresh_token", errorCode);
    }
}
