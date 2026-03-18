using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/user")]
[Authorize]
public class UserController(IClusterClient client, ILogger<UserController> logger) : ControllerBase
{
    private readonly IClusterClient _client = client;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest(ResponseData.Fail(code: "invalid_token", message: "Invalid token."));
        }

        var userGrain = _client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(userId, cancellationToken);
        
        if (user == null)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(data: user));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(id, cancellationToken);
        
        if (user == null)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(data: user));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var result = await userGrain.GetUsersAsync(keyword, pageIndex, pageSize, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        await userGrain.CreateUserAsync(input, cancellationToken);
        return Ok(ResponseData.Success(message: "User created successfully."));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        await userGrain.UpdateUserAsync(id, input, cancellationToken);
        return Ok(ResponseData.Success(message: "User updated successfully."));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        await userGrain.DeleteUserAsync(id, cancellationToken);
        return Ok(ResponseData.Success(message: "User deleted successfully."));
    }
    
    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var result = await userGrain.AssignRolesAsync(id, roleIds, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: "Roles assigned successfully."));
    }
    
    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var roles = await userGrain.GetUserRolesAsync(id, cancellationToken);
        return Ok(ResponseData.Success(data: roles));
    }
    
    [HttpGet("{id:guid}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var permissions = await userGrain.GetUserPermissionsAsync(id, cancellationToken);
        return Ok(ResponseData.Success(data: permissions));
    }
    
    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> DisableUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var result = await userGrain.DisableUserAsync(id, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: "User disabled successfully."));
    }
    
    [HttpPost("{id:guid}/enable")]
    public async Task<IActionResult> EnableUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var result = await userGrain.EnableUserAsync(id, cancellationToken);
        
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "user_not_found", message: "User not found."));
        }
        
        return Ok(ResponseData.Success(message: "User enabled successfully."));
    }
    
    [HttpGet("check/account/{account}")]
    public async Task<IActionResult> CheckAccountExists(string account, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var exists = await userGrain.AccountExistedAsync(account, cancellationToken);
        return Ok(ResponseData.Success(data: exists));
    }
    
    [HttpGet("check/email/{email}")]
    public async Task<IActionResult> CheckEmailExists(string email, CancellationToken cancellationToken = default)
    {
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var exists = await userGrain.EmailExistedAsync(email, cancellationToken);
        return Ok(ResponseData.Success(data: exists));
    }
}
