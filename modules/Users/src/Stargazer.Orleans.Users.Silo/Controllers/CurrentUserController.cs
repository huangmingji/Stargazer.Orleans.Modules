using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/current-user")]
[Authorize]
public class CurrentUserController(IClusterClient client, ILogger<CurrentUserController> logger) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("invalid_token");
        }
        return userId;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<UserDataDto> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(userId, cancellationToken);
        
        if (user == null)
        {
            throw new KeyNotFoundException("user_not_found");
        }
        
        return user;
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<UserDataDto> UpdateCurrentUser([FromBody] UpdateProfileInputDto input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        
        try
        {
            var user = await userGrain.UpdateProfileAsync(userId, input, cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("user_not_found");
            }
            
            return user;
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("update_failed", ex);
        }
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task ChangePassword([FromBody] ChangePasswordInputDto input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);

        await userGrain.VerifyPasswordAsync(new VerifyPasswordInputDto
        {
            Account = (await userGrain.GetUserDataAsync(userId, cancellationToken))?.Account ?? "",
            Password = input.OldPassword
        }, cancellationToken);

        try
        {
            await userGrain.ChangePasswordAsync(userId, input, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("password_change_failed", ex);
        }
    }

    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleDataDto>))]
    public async Task<List<RoleDataDto>> GetCurrentUserRoles(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        return await userGrain.GetUserRolesAsync(userId, cancellationToken);
    }

    [HttpGet("permissions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<List<PermissionDataDto>> GetCurrentUserPermissions(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        return await userGrain.GetUserPermissionsAsync(userId, cancellationToken);
    }

    [HttpGet("has-permission/{permission}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task HasPermission(string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("invalid_permission");
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var hasPermission = await userGrain.HasPermissionAsync(userId, permission, cancellationToken);
        if (!hasPermission)
        {
            throw new InvalidOperationException("permission_denied");
        }
    }
}
