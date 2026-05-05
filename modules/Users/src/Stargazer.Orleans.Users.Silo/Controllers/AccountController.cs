using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Stargazer.Orleans.Users.Silo.Security;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/account")]
public class AccountController(
    IClusterClient client, 
    ILogger<AccountController> logger,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<TokenResponseDto> LoginAsync([FromBody] VerifyPasswordInputDto input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }

        var userGrain = client.GetGrain<IUserGrain>(0);

        var user = await userGrain.VerifyPasswordAsync(input, cancellationToken);
        var roles = await userGrain.GetUserRolesAsync(user.Id, cancellationToken);
        var roleNames = roles.Select(r => r.Name).ToList();

        var (accessToken, refreshToken, expires) = jwtTokenService.GenerateTokens(user.Id, user.Account, roleNames);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            User = user
        };
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<TokenResponseDto> RegisterAsync([FromBody] RegisterAccountInputDto input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }
        
        var userGrain = client.GetGrain<IUserGrain>(0);
        if (await userGrain.AccountExistedAsync(input.Account, cancellationToken))
        {
            throw new InvalidOperationException("account_exists");
        }

        var user = await userGrain.RegisterAsync(input, cancellationToken);
        
        var (accessToken, refreshToken, expires) = jwtTokenService.GenerateTokens(user.Id, user.Account, new List<string>());

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            User = user
        };
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<TokenResponseDto> RefreshTokenAsync([FromBody] RefreshTokenInputDto input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }
        
        var principal = jwtTokenService.ValidateToken(input.RefreshToken);
        if (principal == null)
        {
            throw new InvalidOperationException("invalid_refresh_token");
        }

        var userIdClaim = principal.FindFirst("userId");
        var accountClaim = principal.FindFirst("account");
        
        if (userIdClaim == null || accountClaim == null)
        {
            throw new InvalidOperationException("invalid_token");
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("invalid_user_id");
        }

        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("user_not_found");
        }

        var roles = await userGrain.GetUserRolesAsync(userId, cancellationToken);
        var roleNames = roles.Select(r => r.Name).ToList();
        
        var (accessToken, refreshToken, expires) = jwtTokenService.GenerateTokens(userId, user.Account, roleNames);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            User = user
        };
    }
}
