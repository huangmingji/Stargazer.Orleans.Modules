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
    public async Task<IActionResult> LoginAsync([FromBody] VerifyPasswordInputDto input, CancellationToken cancellationToken)
    { 
        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_input", message: "Invalid input data."));
        }
        
        var userGrain = client.GetGrain<IUserGrain>(0);
        if (await userGrain.VerifyPasswordAsync(input, cancellationToken))
        {
            var user = await userGrain.GetUserDataAsync(input.Account, cancellationToken);
            if (user == null)
            {
                return BadRequest(ResponseData.Fail(code: "user_not_found", message: "User not found."));
            }

            var roles = await userGrain.GetUserRolesAsync(user.Id, cancellationToken);
            var roleNames = roles.Select(r => r.Name).ToList();
            
            var (accessToken, refreshToken) = jwtTokenService.GenerateTokens(user.Id, user.Account, roleNames);

            var response = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user
            };

            return Ok(response);
        }

        return BadRequest(ResponseData.Fail(code: "account_password_incorrect", message: "The account or password is incorrect."));
    }
    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterAccountInputDto input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_input", message: "Invalid input data."));
        }
        
        var userGrain = client.GetGrain<IUserGrain>(0);
        if (await userGrain.AccountExistedAsync(input.Account, cancellationToken))
        {
            return BadRequest(ResponseData.Fail(code:"account_exists", message: "The account already exists"));
        }

        var user = await userGrain.RegisterAsync(input, cancellationToken);
        
        var (accessToken, refreshToken) = jwtTokenService.GenerateTokens(user.Id, user.Account, new List<string>());

        var response = new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        return Ok(response);
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenInputDto input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_input", message: "Invalid input data."));
        }
        
        var principal = jwtTokenService.ValidateToken(input.RefreshToken);
        if (principal == null)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_refresh_token", message: "Invalid refresh token."));
        }

        var userIdClaim = principal.FindFirst("userId");
        var accountClaim = principal.FindFirst("account");
        
        if (userIdClaim == null || accountClaim == null)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_token", message: "Invalid token claims."));
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest(ResponseData.Fail(code: "invalid_user_id", message: "Invalid user ID."));
        }

        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(userId, cancellationToken);
        if (user == null)
        {
            return BadRequest(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }

        var roles = await userGrain.GetUserRolesAsync(userId, cancellationToken);
        var roleNames = roles.Select(r => r.Name).ToList();
        
        var (accessToken, refreshToken) = jwtTokenService.GenerateTokens(userId, user.Account, roleNames);

        var response = new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        return Ok(response);
    }
}
