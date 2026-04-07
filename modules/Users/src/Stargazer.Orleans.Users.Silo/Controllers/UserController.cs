using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/user")]
[Authorize]
public class UserController(IClusterClient client, ILogger<UserController> logger) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(id, cancellationToken);
        
        if (user == null)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(data: user));
    }
    
    [HttpGet]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.GetUsersAsync(keyword, pageIndex, pageSize, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Create}")]
    public async Task<IActionResult> CreateUser([FromBody] CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        await userGrain.CreateUserAsync(input, cancellationToken);
        return Ok(ResponseData.Success(message: "User created successfully."));
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Update}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        await userGrain.UpdateUserAsync(id, input, cancellationToken);
        return Ok(ResponseData.Success(message: "User updated successfully."));
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Delete}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.DeleteUserAsync(id, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: "User deleted successfully."));
    }
    
    [HttpPost("{id:guid}/roles")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Assign}")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.AssignRolesAsync(id, roleIds, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: "Roles assigned successfully."));
    }
    
    [HttpGet("{id:guid}/roles")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    public async Task<IActionResult> GetUserRoles(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var roles = await userGrain.GetUserRolesAsync(id, cancellationToken);
        return Ok(ResponseData.Success(data: roles));
    }
    
    [HttpGet("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    public async Task<IActionResult> GetUserPermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var permissions = await userGrain.GetUserPermissionsAsync(id, cancellationToken);
        return Ok(ResponseData.Success(data: permissions));
    }
    
    [HttpPatch("{id:guid}/status")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Update}")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.UpdateUserStatusAsync(id, input, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: $"User {(input.IsEnabled ? "enabled" : "disabled")} successfully."));
    }
    
    // [HttpGet("check/account/{account}")]
    // public async Task<IActionResult> CheckAccountExists(string account, CancellationToken cancellationToken = default)
    // {
    //     var userGrain = _client.GetGrain<IUserGrain>(0);
    //     var exists = await userGrain.AccountExistedAsync(account, cancellationToken);
    //     return Ok(ResponseData.Success(data: exists));
    // }
    //
    // [HttpGet("check/email/{email}")]
    // public async Task<IActionResult> CheckEmailExists(string email, CancellationToken cancellationToken = default)
    // {
    //     var userGrain = _client.GetGrain<IUserGrain>(0);
    //     var exists = await userGrain.EmailExistedAsync(email, cancellationToken);
    //     return Ok(ResponseData.Success(data: exists));
    // }
    //
    // [HttpPost("has-permission")]
    // public async Task<IActionResult> HasPermission([FromBody] HasPermissionRequest request, CancellationToken cancellationToken = default)
    // {
    //     if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Permission))
    //     {
    //         return BadRequest(ResponseData.Fail(code: "invalid_request", message: "UserId and Permission are required."));
    //     }
    //
    //     var userGrain = _client.GetGrain<IUserGrain>(0);
    //     var hasPermission = await userGrain.HasPermissionAsync(request.UserId, request.Permission, cancellationToken);
    //     return Ok(ResponseData.Success(data: hasPermission));
    // }
}
//
// public class HasPermissionRequest
// {
//     public Guid UserId { get; set; }
//     public string Permission { get; set; } = string.Empty;
// }
