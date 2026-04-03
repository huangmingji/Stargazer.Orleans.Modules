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
    public async Task<IActionResult> GetRoles([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.GetRolesAsync(keyword, pageIndex, pageSize, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.GetRoleAsync(id, cancellationToken);
        if (role == null)
        {
            return NotFound(ResponseData.Fail(code: "role_not_found", message: "Role not found."));
        }
        return Ok(ResponseData.Success(data: role));
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Create}")]
    public async Task<IActionResult> CreateRole([FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.CreateRoleAsync(input, cancellationToken);
        return Ok(ResponseData.Success(data: role));
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Update}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var role = await roleGrain.UpdateRoleAsync(id, input, cancellationToken);
        return Ok(ResponseData.Success(data: role));
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Delete}")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.DeleteRoleAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "role_not_found", message: "Role not found."));
        }
        return Ok(ResponseData.Success(data: result));
    }
    
    [HttpGet("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    public async Task<IActionResult> GetRolePermissions(Guid id, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var permissions = await roleGrain.GetPermissionsAsync(id, cancellationToken);
        return Ok(ResponseData.Success(data: permissions));
    }
    
    [HttpPost("{id:guid}/permissions")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.Assign}")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var result = await roleGrain.AssignPermissionsAsync(id, permissionIds, cancellationToken);
        return Ok(ResponseData.Success(data: result));
    }
    
    [HttpGet("active")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Roles.View}")]
    public async Task<IActionResult> GetActiveRoles(CancellationToken cancellationToken = default)
    {
        var roleGrain = client.GetGrain<IRoleGrain>(0);
        var roles = await roleGrain.GetActiveRolesAsync(cancellationToken);
        return Ok(ResponseData.Success(data: roles));
    }
}
