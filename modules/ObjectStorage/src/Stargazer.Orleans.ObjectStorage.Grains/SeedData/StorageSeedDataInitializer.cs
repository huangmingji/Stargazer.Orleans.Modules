using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.ObjectStorage.Grains.SeedData;

public class StorageSeedDataInitializer(
    IClusterClient clusterClient,
    ILogger<StorageSeedDataInitializer> logger) : IStartupTask
{
    private const string AdminAccount = "admin";
    private const string ObjectStorageAdminRoleName = "ObjectStorageAdmin";
    private const string ObjectStorageAdminRoleDescription = "对象存储管理员，拥有所有对象存储权限";

    private IPermissionGrain PermissionGrain => clusterClient.GetGrain<IPermissionGrain>(0);
    private IRoleGrain RoleGrain => clusterClient.GetGrain<IRoleGrain>(0);
    private IUserGrain UserGrain => clusterClient.GetGrain<IUserGrain>(0);

    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("开始初始化ObjectStorage模块种子数据...");
        
        var createdPermissionIds = await SeedPermissionsAsync(cancellationToken);
        var adminRole = await SeedObjectStorageAdminRoleAsync(createdPermissionIds, cancellationToken);
        await AssignRoleToAdminAsync(adminRole.Id, cancellationToken);
        
        logger.LogInformation("ObjectStorage模块种子数据初始化完成");
    }

    private async Task<List<Guid>> SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissionCodes = new List<(string Name, string Code, string Category, string Description)>
        {
            ("查看存储桶", StoragePolicies.Buckets.View, "存储桶管理", "查看存储桶列表"),
            ("创建存储桶", StoragePolicies.Buckets.Create, "存储桶管理", "创建新存储桶"),
            ("编辑存储桶", StoragePolicies.Buckets.Update, "存储桶管理", "编辑存储桶信息"),
            ("删除存储桶", StoragePolicies.Buckets.Delete, "存储桶管理", "删除存储桶"),
            
            ("查看对象", StoragePolicies.Objects.View, "对象管理", "查看和下载对象"),
            ("上传对象", StoragePolicies.Objects.Create, "对象管理", "上传新对象"),
            ("编辑对象", StoragePolicies.Objects.Update, "对象管理", "更新对象"),
            ("删除对象", StoragePolicies.Objects.Delete, "对象管理", "删除对象")
        };

        var createdPermissionIds = new List<Guid>();

        foreach (var (name, code, category, description) in permissionCodes)
        {
            var existing = await PermissionGrain.GetPermissionByCodeAsync(code, cancellationToken);
            if (existing is not null)
            {
                logger.LogDebug("权限 {Code} 已存在，跳过", code);
                createdPermissionIds.Add(existing.Id);
                continue;
            }

            var permissionDto = new PermissionDataDto
            {
                Id = Guid.Empty,
                Name = name,
                Code = code,
                Category = category,
                Description = description,
                Type = (int)PermissionType.Operation,
                IsActive = true
            };
            
            var created = await PermissionGrain.CreatePermissionAsync(permissionDto, cancellationToken);
            createdPermissionIds.Add(created.Id);
            logger.LogDebug("创建权限: {Name} ({Code})", name, code);
        }

        return createdPermissionIds;
    }

    private async Task<RoleDataDto> SeedObjectStorageAdminRoleAsync(List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var existingRole = await RoleGrain.GetRoleByNameAsync(ObjectStorageAdminRoleName, cancellationToken);
        if (existingRole is not null)
        {
            logger.LogDebug("{RoleName}角色已存在，跳过创建", ObjectStorageAdminRoleName);
            await RoleGrain.AssignPermissionsAsync(existingRole.Id, permissionIds, cancellationToken);
            return existingRole;
        }

        var roleInput = new CreateOrUpdateRoleInputDto
        {
            Name = ObjectStorageAdminRoleName,
            Description = ObjectStorageAdminRoleDescription,
            IsDefault = false,
            Priority = 100,
            IsActive = true,
            PermissionIds = permissionIds
        };
        
        var adminRole = await RoleGrain.CreateRoleAsync(roleInput, cancellationToken);
        logger.LogInformation("创建{RoleName}角色成功: {RoleId}", ObjectStorageAdminRoleName, adminRole.Id);

        return adminRole;
    }

    private async Task AssignRoleToAdminAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var adminUser = await UserGrain.GetUserByAccountAsync(AdminAccount, cancellationToken);
        
        if (adminUser is null)
        {
            logger.LogWarning("未找到Admin用户 ({Account})，跳过角色分配", AdminAccount);
            return;
        }
        
        var existingRoles = await UserGrain.GetUserRolesAsync(adminUser.Id, cancellationToken);
        if (existingRoles.Any(r => r.Id == roleId))
        {
            logger.LogDebug("Admin账号已拥有MessageAdmin角色，跳过分配");
            return;
        }
        var roleIds = existingRoles.Select(r => r.Id).ToList();
        roleIds.Add(roleId);

        await UserGrain.AssignRolesAsync(adminUser.Id, roleIds, cancellationToken);
        logger.LogInformation("为用户 {Account} 分配{RoleName}角色成功", AdminAccount, ObjectStorageAdminRoleName);
    }
}

public enum PermissionType
{
    Operation = 0,
    Menu = 1,
    Button = 2,
    Api = 3
}
