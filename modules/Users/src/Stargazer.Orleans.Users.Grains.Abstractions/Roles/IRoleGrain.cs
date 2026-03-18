using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles;

public interface IRoleGrain : IGrainWithIntegerKey
{
    Task<RoleDataDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<RoleDataDto?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<List<RoleDataDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    
    Task<List<RoleDataDto>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    
    Task<PageResult<RoleDataDto>> GetRolesAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    
    Task<RoleDataDto> CreateRoleAsync(CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default);
    
    Task<RoleDataDto> UpdateRoleAsync(Guid id, CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, CancellationToken cancellationToken = default);
    
    Task<List<PermissionDataDto>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    
    Task<bool> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default);
}
