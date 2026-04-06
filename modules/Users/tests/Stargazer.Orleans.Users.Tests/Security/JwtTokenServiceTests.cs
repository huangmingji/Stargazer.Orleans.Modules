using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Stargazer.Orleans.Users.Silo.Security;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _tokenService;
    private readonly JwtSettings _settings;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes123456789!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };
        _tokenService = new JwtTokenService(_settings);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidToken()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin", "User" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Contains(jwtToken.Claims, c => c.Type == "userId" && c.Value == userId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ContainsAccountClaim()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Contains(jwtToken.Claims, c => c.Type == "account" && c.Value == account);
    }

    [Fact]
    public void GenerateAccessToken_ContainsRoleClaims()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin", "User" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaimTypes = new[] { ClaimTypes.Role, "role" };
        var roleClaims = jwtToken.Claims.Where(c => roleClaimTypes.Contains(c.Type)).Select(c => c.Value).ToList();
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("User", roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(_settings.Issuer, jwtToken.Issuer);
        Assert.Contains(_settings.Audience, jwtToken.Audiences);
    }

    // [Fact]
    // public void GenerateRefreshToken_ReturnsBase64String()
    // {
    //     var refreshToken = _tokenService.GenerateRefreshToken();
    //
    //     Assert.NotNull(refreshToken);
    //     Assert.NotEmpty(refreshToken);
    //     
    //     var bytes = Convert.FromBase64String(refreshToken);
    //     Assert.Equal(64, bytes.Length);
    // }
    //
    // [Fact]
    // public void GenerateRefreshToken_ReturnsUniqueTokens()
    // {
    //     var token1 = _tokenService.GenerateRefreshToken();
    //     var token2 = _tokenService.GenerateRefreshToken();
    //
    //     Assert.NotEqual(token1, token2);
    // }

    [Fact]
    public void GenerateTokens_ReturnsBothTokens()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin" };

        var (accessToken, refreshToken) = _tokenService.GenerateTokens(userId, account, roles);

        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);
    }

    [Fact]
    public void ValidateToken_ReturnsPrincipal_ForValidToken()
    {
        var userId = Guid.NewGuid();
        var account = "testuser";
        var roles = new[] { "Admin" };

        var token = _tokenService.GenerateAccessToken(userId, account, roles);
        var principal = _tokenService.ValidateToken(token);

        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForInvalidToken()
    {
        var principal = _tokenService.ValidateToken("invalid-token");

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForEmptyToken()
    {
        var principal = _tokenService.ValidateToken(string.Empty);

        Assert.Null(principal);
    }
}

public class JwtSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new JwtSettings();

        Assert.Equal(string.Empty, settings.SecretKey);
        Assert.Equal("Stargazer.Orleans.Users", settings.Issuer);
        Assert.Equal("Stargazer.Orleans.Users", settings.Audience);
        Assert.Equal(60, settings.ExpiryMinutes);
        Assert.Equal(7, settings.RefreshTokenExpiryDays);
    }

    [Fact]
    public void CanSetCustomValues()
    {
        var settings = new JwtSettings
        {
            SecretKey = "custom-secret-key",
            Issuer = "CustomIssuer",
            Audience = "CustomAudience",
            ExpiryMinutes = 120,
            RefreshTokenExpiryDays = 30
        };

        Assert.Equal("custom-secret-key", settings.SecretKey);
        Assert.Equal("CustomIssuer", settings.Issuer);
        Assert.Equal("CustomAudience", settings.Audience);
        Assert.Equal(120, settings.ExpiryMinutes);
        Assert.Equal(30, settings.RefreshTokenExpiryDays);
    }
}
