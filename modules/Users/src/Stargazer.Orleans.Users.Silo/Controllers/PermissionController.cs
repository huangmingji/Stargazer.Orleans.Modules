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
    [HttpGet]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<PermissionDataDto>))]
    public async Task<PageResult<PermissionDataDto>> GetPermissions([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        return await permissionGrain.GetPermissionsAsync(keyword, pageIndex, pageSize, cancellationToken);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<PermissionDataDto> GetPermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        var permission = await permissionGrain.GetPermissionAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new KeyNotFoundException("permission_not_found");
        }
        return permission;
    }
    
    [HttpPost]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Create}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    public async Task<PermissionDataDto> CreatePermission([FromBody] PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        return await permissionGrain.CreatePermissionAsync(input, cancellationToken);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Update}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionDataDto))]
    public async Task<PermissionDataDto> UpdatePermission(Guid id, [FromBody] PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        return await permissionGrain.UpdatePermissionAsync(id, input, cancellationToken);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task DeletePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        var result = await permissionGrain.DeletePermissionAsync(id, cancellationToken);
        if (!result)
        {
            throw new KeyNotFoundException("permission_not_found");
        }
    }
    
    [HttpGet("category/{category}")]
    [Authorize(policy: $"permission:{AuthorizationPermissions.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PermissionDataDto>))]
    public async Task<List<PermissionDataDto>> GetPermissionsByCategory(string category, CancellationToken cancellationToken = default)
    {
        var permissionGrain = client.GetGrain<IPermissionGrain>(0);
        return await permissionGrain.GetPermissionsByCategoryAsync(category, cancellationToken);
    }
}
