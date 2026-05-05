using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Stargazer.Orleans.Users.Grains.Abstractions.Security;

namespace Stargazer.Orleans.Users.Silo.Security;

public interface IJwtTokenService
{
    (string token, DateTime expires) GenerateAccessToken(Guid userId, string account, IEnumerable<string> roles);
    (string token, DateTime expires) GenerateRefreshToken(Guid userId, string account);
    ClaimsPrincipal? ValidateToken(string token);
    (string accessToken, string refreshToken, DateTime expires) GenerateTokens(Guid userId, string account, IEnumerable<string> roles);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly Grains.Abstractions.Security.JwtSettings _settings;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(Grains.Abstractions.Security.JwtSettings settings)
    {
        _settings = settings;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _securityKey
        };
    }

    public (string token, DateTime expires) GenerateAccessToken(Guid userId, string account, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Name, account),
            new("userId", userId.ToString()),
            new("account", account)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        DateTime expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expires);
    }

    public (string token, DateTime expires) GenerateRefreshToken(Guid userId, string account)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("userId", userId.ToString()),
            new("account", account)
        };

        DateTime expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expires);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            return validatedToken is JwtSecurityToken jwtToken && jwtToken.ValidTo > DateTime.UtcNow
                ? principal
                : null;
        }
        catch
        {
            return null;
        }
    }

    public (string accessToken, string refreshToken, DateTime expires) GenerateTokens(Guid userId, string account, IEnumerable<string> roles)
    {
        var (accessToken, expires) = GenerateAccessToken(userId, account, roles);
        var (refreshToken, _) = GenerateRefreshToken(userId, account);
        return (accessToken, refreshToken, expires);
    }
}
