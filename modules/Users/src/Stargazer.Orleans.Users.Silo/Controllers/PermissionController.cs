using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/permission")]
[Authorize]
public class PermissionController(IClusterClient client, ILogger<PermissionController> logger) : ControllerBase
{
    private readonly IClusterClient _client = client;
    
    [HttpGet]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<PermissionDataDto>))]
    public async Task<IActionResult> GetPermissions([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var result = await permissionGrain.GetPermissionsAsync(keyword, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<IActionResult> GetPermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var permission = await permissionGrain.GetPermissionAsync(id, cancellationToken);
        if (permission == null)
        {
            return NotFound(ResponseData.Fail(code: "permission_not_found", message: "Permission not found."));
        }
        return Ok(permission);
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var permission = await permissionGrain.CreatePermissionAsync(input, cancellationToken);
        return Ok(permission);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var permission = await permissionGrain.UpdatePermissionAsync(id, input, cancellationToken);
        return Ok(permission);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<IActionResult> DeletePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var result = await permissionGrain.DeletePermissionAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ResponseData.Fail(code: "permission_not_found", message: "Permission not found."));
        }
        return Ok();
    }
    
    [HttpGet("category/{category}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<IActionResult> GetPermissionsByCategory(string category, CancellationToken cancellationToken = default)
    {
        var permissionGrain = _client.GetGrain<IPermissionGrain>(0);
        var permissions = await permissionGrain.GetPermissionsByCategoryAsync(category, cancellationToken);
        return Ok(permissions);
    }
}
