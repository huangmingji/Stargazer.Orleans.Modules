using System.Linq.Expressions;
using Orleans.Concurrency;
using Stargazer.Common;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Roles;

[StatelessWorker]
public class PermissionGrain(
    IRepository<PermissionData, Guid> permissionRepository,
    IRepository<RoleData, Guid> roleRepository,
    IRepository<RolePermissionData, Guid> rolePermissionRepository) : Grain, IPermissionGrain
{
    public async Task<PermissionDataDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.FindAsync(id, cancellationToken);
        return permission?.MapToPermissionDto();
    }

    public async Task<PermissionDataDto?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.FindAsync(x => x.Code == code, cancellationToken);
        return permission?.MapToPermissionDto();
    }

    public async Task<List<PermissionDataDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await permissionRepository.FindAllAsync(cancellationToken);
        return permissions.Select(x => x.MapToPermissionDto()).ToList();
    }

    public async Task<List<PermissionDataDto>> GetPermissionsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var permissions = await permissionRepository.FindListAsync(x => x.Category == category, cancellationToken);
        return permissions.Select(x => x.MapToPermissionDto()).ToList();
    }

    public async Task<PageResult<PermissionDataDto>> GetPermissionsAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        Expression<Func<PermissionData, bool>>? predicate = null;
        if (!string.IsNullOrEmpty(keyword))
        {
            predicate = x => x.Name.Contains(keyword) || x.Code.Contains(keyword) || x.Category.Contains(keyword);
        }
        
        var (items, total) = await permissionRepository.FindListAsync(
            predicate,
            pageIndex,
            pageSize,
            orderBy: x => x.Name,
            orderByDescending: false,
            cancellationToken: cancellationToken);
        
        return new PageResult<PermissionDataDto>
        {
            Total = total,
            Items = items.Select(x => x.MapToPermissionDto()).ToList()
        };
    }

    public async Task<PermissionDataDto> CreatePermissionAsync(PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permission = new PermissionData
        {
            Id = input.Id == Guid.Empty ? new SequentialGuid().Create() : input.Id,
            Name = input.Name,
            Code = input.Code,
            Description = input.Description,
            Category = input.Category,
            Type = (PermissionType)input.Type,
            IsActive = input.IsActive,
            CreationTime = DateTime.UtcNow
        };
        
        var result = await permissionRepository.InsertAsync(permission, cancellationToken);
        return result.MapToPermissionDto();
    }

    public async Task<PermissionDataDto> UpdatePermissionAsync(Guid id, PermissionDataDto input, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.GetAsync(id, cancellationToken);
        permission.Name = input.Name;
        permission.Code = input.Code;
        permission.Description = input.Description;
        permission.Category = input.Category;
        permission.Type = (PermissionType)input.Type;
        permission.IsActive = input.IsActive;
        permission.LastModifyTime = DateTime.UtcNow;
        
        var result = await permissionRepository.UpdateAsync(permission, cancellationToken);
        return result.MapToPermissionDto();
    }

    public async Task<bool> DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.FindAsync(id, cancellationToken);
        if (permission is null) return false;
        
        var rolePermissions = await rolePermissionRepository.FindListAsync(x => x.PermissionId == id, cancellationToken);
        if (rolePermissions.Any())
        {
            await rolePermissionRepository.DeleteManyAsync(rolePermissions.Select(x => x.Id), cancellationToken);
        }
        
        await permissionRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
