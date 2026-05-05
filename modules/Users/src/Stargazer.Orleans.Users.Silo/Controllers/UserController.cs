using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<UserDataDto> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var user = await userGrain.GetUserDataAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("user_not_found");
        }
        return user;
    }
    
    [HttpGet]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<UserDataDto>))]
    public async Task<PageResult<UserDataDto>> GetUsers([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        return await userGrain.GetUsersAsync(keyword, pageIndex, pageSize, cancellationToken);
    }

    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task CreateUser([FromBody] CreateOrUpdateUserInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }

        var userGrain = client.GetGrain<IUserGrain>(0);
        await userGrain.CreateUserAsync(input, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task UpdateUser(Guid id, [FromBody] CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException("invalid_input");
        }
        
        var userGrain = client.GetGrain<IUserGrain>(0);
        await userGrain.UpdateUserAsync(id, input, cancellationToken);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.DeleteUserAsync(id, cancellationToken);
        if (!result)
        {
            throw new KeyNotFoundException("user_not_found");
        }
    }
    
    [HttpPost("{id:guid}/roles")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Assign}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task AssignRoles(Guid id, [FromBody] List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.AssignRolesAsync(id, roleIds, cancellationToken);
        
        if (!result)
        {
            throw new KeyNotFoundException("user_not_found");
        }
    }
    
    [HttpGet("{id:guid}/roles")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleDataDto>))]
    public async Task<List<RoleDataDto>> GetUserRoles(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        return await userGrain.GetUserRolesAsync(id, cancellationToken);
    }
    
    [HttpGet("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<List<PermissionDataDto>> GetUserPermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        return await userGrain.GetUserPermissionsAsync(id, cancellationToken);
    }
    
    [HttpPatch("{id:guid}/status")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Users.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusInputDto input, CancellationToken cancellationToken = default)
    {
        var userGrain = client.GetGrain<IUserGrain>(0);
        var result = await userGrain.UpdateUserStatusAsync(id, input, cancellationToken);
        
        if (!result)
        {
            throw new KeyNotFoundException("user_not_found");
        }
    }
}
