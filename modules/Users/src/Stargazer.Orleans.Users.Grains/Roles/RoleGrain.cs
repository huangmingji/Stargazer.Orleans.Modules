using System.Linq.Expressions;
using Orleans.Concurrency;
using Stargazer.Common;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Roles;

[StatelessWorker]
public class RoleGrain(
    IRepository<RoleData, Guid> roleRepository,
    IRepository<PermissionData, Guid> permissionRepository,
    IRepository<UserRoleData, Guid> userRoleRepository,
    IRepository<RolePermissionData, Guid> rolePermissionRepository) : Grain, IRoleGrain
{
    public async Task<RoleDataDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.FindAsync(id, cancellationToken);
        return role?.MapToRoleDto();
    }

    public async Task<RoleDataDto?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.FindAsync(x => x.Name == name, cancellationToken);
        return role?.MapToRoleDto();
    }

    public async Task<List<RoleDataDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.FindAllAsync(cancellationToken);
        return roles.Select(x => x.MapToRoleDto()).ToList();
    }

    public async Task<List<RoleDataDto>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.FindListAsync(x => x.IsActive, cancellationToken);
        return roles.Select(x => x.MapToRoleDto()).ToList();
    }

    public async Task<PageResult<RoleDataDto>> GetRolesAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        Expression<Func<RoleData, bool>>? predicate = null;
        if (!string.IsNullOrEmpty(keyword))
        {
            predicate = x => x.Name.Contains(keyword) || x.Description.Contains(keyword);
        }
        
        var (items, total) = await roleRepository.FindListAsync(
            predicate,
            pageIndex,
            pageSize,
            orderBy: x => x.Priority,
            orderByDescending: true,
            cancellationToken: cancellationToken);
        
        return new PageResult<RoleDataDto>
        {
            Total = total,
            Items = items.Select(x => x.MapToRoleDto()).ToList()
        };
    }

    public async Task<RoleDataDto> CreateRoleAsync(CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var role = new RoleData
        {
            Id = new SequentialGuid().Create(),
            Name = input.Name,
            Description = input.Description,
            IsDefault = input.IsDefault,
            Priority = input.Priority,
            IsActive = input.IsActive,
            CreationTime = DateTime.UtcNow
        };
        
        if (input.PermissionIds.Any())
        {
            foreach (var permissionId in input.PermissionIds)
            {
                var rolePermission = new RolePermissionData
                {
                    Id = new SequentialGuid().Create(),
                    RoleId = role.Id,
                    PermissionId = permissionId,
                    CreationTime = DateTime.UtcNow
                };
                role.RolePermissions.Add(rolePermission);
            }
        }
        
        var result = await roleRepository.InsertAsync(role, cancellationToken);
        return result.MapToRoleDto();
    }

    public async Task<RoleDataDto> UpdateRoleAsync(Guid id, CreateOrUpdateRoleInputDto input, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetAsync(id, cancellationToken);
        role.Name = input.Name;
        role.Description = input.Description;
        role.IsDefault = input.IsDefault;
        role.Priority = input.Priority;
        role.IsActive = input.IsActive;
        role.LastModifyTime = DateTime.UtcNow;
        
        var existingRolePermissions = await rolePermissionRepository.FindListAsync(x => x.RoleId == id, cancellationToken);
        if (existingRolePermissions.Any())
        {
            await rolePermissionRepository.DeleteManyAsync(existingRolePermissions.Select(x => x.Id), cancellationToken);
        }
        
        if (input.PermissionIds.Any())
        {
            foreach (var permissionId in input.PermissionIds)
            {
                var rolePermission = new RolePermissionData
                {
                    Id = new SequentialGuid().Create(),
                    RoleId = role.Id,
                    PermissionId = permissionId,
                    CreationTime = DateTime.UtcNow
                };
                await rolePermissionRepository.InsertAsync(rolePermission, cancellationToken);
            }
        }
        
        var result = await roleRepository.UpdateAsync(role, cancellationToken);
        return result.MapToRoleDto();
    }

    public async Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.FindAsync(id, cancellationToken);
        if (role is null) return false;
        
        var userRoles = await userRoleRepository.FindListAsync(x => x.RoleId == id, cancellationToken);
        if (userRoles.Any())
        {
            await userRoleRepository.DeleteManyAsync(userRoles.Select(x => x.Id), cancellationToken);
        }
        
        var rolePermissions = await rolePermissionRepository.FindListAsync(x => x.RoleId == id, cancellationToken);
        if (rolePermissions.Any())
        {
            await rolePermissionRepository.DeleteManyAsync(rolePermissions.Select(x => x.Id), cancellationToken);
        }
        
        await roleRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetAsync(roleId, cancellationToken);
        
        var existingRolePermissions = await rolePermissionRepository.FindListAsync(x => x.RoleId == roleId, cancellationToken);
        if (existingRolePermissions.Any())
        {
            await rolePermissionRepository.DeleteManyAsync(existingRolePermissions.Select(x => x.Id), cancellationToken);
        }
        
        foreach (var permissionId in permissionIds)
        {
            var rolePermission = new RolePermissionData
            {
                Id = new SequentialGuid().Create(),
                RoleId = roleId,
                PermissionId = permissionId,
                CreationTime = DateTime.UtcNow
            };
            await rolePermissionRepository.InsertAsync(rolePermission, cancellationToken);
        }
        
        role.LastModifyTime = DateTime.UtcNow;
        await roleRepository.UpdateAsync(role, cancellationToken);
        return true;
    }

    public async Task<List<PermissionDataDto>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await rolePermissionRepository.FindListAsync(x => x.RoleId == roleId, cancellationToken);
        var permissionIds = rolePermissions.Select(x => x.PermissionId).ToList();
        
        if (!permissionIds.Any())
            return new List<PermissionDataDto>();
        
        var permissions = await permissionRepository.FindListAsync(x => permissionIds.Contains(x.Id), cancellationToken);
        return permissions.Select(x => x.MapToPermissionDto()).ToList();
    }

    public async Task<bool> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await rolePermissionRepository.FindListAsync(x => x.RoleId == roleId, cancellationToken);
        var permissionIds = rolePermissions.Select(x => x.PermissionId).ToList();
        
        if (!permissionIds.Any())
            return false;
        
        var permission = await permissionRepository.FindAsync(x => permissionIds.Contains(x.Id) && x.Code == permissionCode && x.IsActive, cancellationToken);
        return permission is not null;
    }
}
