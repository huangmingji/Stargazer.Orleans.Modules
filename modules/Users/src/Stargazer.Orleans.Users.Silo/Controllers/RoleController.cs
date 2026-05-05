using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/role")]
[Authorize]
public class RoleController(IClusterClient client, ILogger<RoleController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<RoleDataDto>))]
    public async Task<PageResult<RoleDataDto>> GetRoles([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        return await roleGrain.GetRolesAsync(keyword, pageIndex, pageSize, cancellationToken);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<RoleDataDto> GetRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.GetRoleAsync(id, cancellationToken);
        if (role == null)
        {
            throw new KeyNotFoundException("role_not_found");
        }
        return role;
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    public async Task<RoleDataDto> CreateRole([FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        return await roleGrain.CreateRoleAsync(input, cancellationToken);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    public async Task<RoleDataDto> UpdateRole(Guid id, [FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        return await roleGrain.UpdateRoleAsync(id, input, cancellationToken);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task DeleteRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.DeleteRoleAsync(id, cancellationToken);
        if (!result)
        {
            throw new KeyNotFoundException("role_not_found");
        }
    }
    
    [HttpGet("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<List<PermissionDataDto>> GetRolePermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        return await roleGrain.GetPermissionsAsync(id, cancellationToken);
    }
    
    [HttpPost("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Assign}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        if (!await roleGrain.AssignPermissionsAsync(id, permissionIds, cancellationToken))
        {
            throw new InvalidOperationException("role_not_found");
        }
    }
    
    [HttpGet("active")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleDataDto>))]
    public async Task<List<RoleDataDto>> GetActiveRoles(CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        return await roleGrain.GetActiveRolesAsync(cancellationToken);
    }
}
