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
            throw new UnauthorizedAccessException("Invalid token");
        }
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(userId, cancellationToken);
        
        if (user == null)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(data: user));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileInputDto input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_input", message: "Invalid input data."));
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        
        try
        {
            var user = await userGrain.UpdateProfileAsync(userId, input, cancellationToken);
            
            if (user == null)
            {
                return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
            }
            
            return Ok(ResponseData.Success(data: user, message: "Profile updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseData.Fail(code: "update_failed", message: ex.Message));
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordInputDto input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_input", message: "Invalid input data."));
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);

        var isCurrentPasswordValid = await userGrain.VerifyPasswordAsync(new VerifyPasswordInputDto
        {
            Name = (await userGrain.GetUserDataAsync(userId, cancellationToken))?.Account ?? "",
            Password = input.OldPassword
        }, cancellationToken);

        if (!isCurrentPasswordValid)
        {
            return BadRequest(ResponseData.Fail(code: "invalid_password", message: "Current password is incorrect."));
        }

        try
        {
            await userGrain.ChangePasswordAsync(userId, input, userId, cancellationToken);
            return Ok(ResponseData.Success(message: "Password changed successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseData.Fail(code: "password_change_failed", message: ex.Message));
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetCurrentUserRoles(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var roles = await userGrain.GetUserRolesAsync(userId, cancellationToken);
        
        return Ok(ResponseData.Success(data: roles));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetCurrentUserPermissions(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var permissions = await userGrain.GetUserPermissionsAsync(userId, cancellationToken);
        
        return Ok(ResponseData.Success(data: permissions));
    }

    [HttpGet("has-permission/{permission}")]
    public async Task<IActionResult> HasPermission(string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return BadRequest(ResponseData.Fail(code: "invalid_permission", message: "Permission code is required."));
        }

        var userId = GetCurrentUserId();
        var userGrain = client.GetGrain<IUserGrain>(0);
        var hasPermission = await userGrain.HasPermissionAsync(userId, permission, cancellationToken);
        
        return Ok(ResponseData.Success(data: hasPermission));
    }
}
