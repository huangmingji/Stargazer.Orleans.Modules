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
    public async Task<IActionResult> GetRoles([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.GetRolesAsync(keyword, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.GetRoleAsync(id, cancellationToken);
        if (role == null)
        {
            return NotFound(ResponseData.Fail(code: "role_not_found", message: "Role not found."));
        }
        return Ok(role);
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    public async Task<IActionResult> CreateRole([FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.CreateRoleAsync(input, cancellationToken);
        return Ok(role);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDataDto))]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.UpdateRoleAsync(id, input, cancellationToken);
        return Ok(role);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.DeleteRoleAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "role_not_found", message: "Role not found."));
        }
        return Ok();
    }
    
    [HttpGet("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<IActionResult> GetRolePermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var permissions = await roleGrain.GetPermissionsAsync(id, cancellationToken);
        return Ok(permissions);
    }
    
    [HttpPost("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Assign}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.AssignPermissionsAsync(id, permissionIds, cancellationToken);
        if (result)
        {
            return Ok();
        }
        return BadRequest(ResponseData.Fail());
    }
    
    [HttpGet("active")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleDataDto>))]
    public async Task<IActionResult> GetActiveRoles(CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var roles = await roleGrain.GetActiveRolesAsync(cancellationToken);
        return Ok(roles);
    }
}
