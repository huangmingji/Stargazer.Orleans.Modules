using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles;

public interface IPermissionGrain : IGrainWithIntegerKey
{
    Task<PermissionDataDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PermissionDataDto?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default);
    
    Task<List<PermissionDataDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    
    Task<List<PermissionDataDto>> GetPermissionsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    
    Task<PageResult<PermissionDataDto>> GetPermissionsAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    
    Task<PermissionDataDto> CreatePermissionAsync(PermissionDataDto input, CancellationToken cancellationToken = default);
    
    Task<PermissionDataDto> UpdatePermissionAsync(Guid id, PermissionDataDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default);
}
