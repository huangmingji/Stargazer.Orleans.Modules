using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.MessageManagement.Grains.SeedData;

public class MessageManagementSeedDataInitializer(
    IClusterClient clusterClient,
    ILogger<MessageManagementSeedDataInitializer> logger) : IStartupTask
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("开始初始化MessageManagement模块种子数据...");
        
        var permissionGrain = clusterClient.GetGrain<IPermissionGrain>(0);
        var roleGrain = clusterClient.GetGrain<IRoleGrain>(0);
        var userGrain = clusterClient.GetGrain<IUserGrain>(0);
        
        await SeedPermissionsAsync(permissionGrain, cancellationToken);
        var messageAdminRoleId = await CreateMessageAdminRoleAsync(permissionGrain, roleGrain, cancellationToken);
        await AssignMessageAdminRoleToAdminUserAsync(userGrain, roleGrain, messageAdminRoleId, cancellationToken);
        
        logger.LogInformation("MessageManagement模块种子数据初始化完成");
    }

    private async Task SeedPermissionsAsync(IPermissionGrain permissionGrain, CancellationToken cancellationToken)
    {
        var permissions = new List<(string Name, string Code, string Category, string Description)>
        {
            ("发送消息", AuthorizationPermissions.Messages.Send, "消息管理", "发送消息"),
            ("查看消息", AuthorizationPermissions.Messages.View, "消息管理", "查看消息列表"),
            ("重试消息", AuthorizationPermissions.Messages.Retry, "消息管理", "重试发送失败的消息"),
            ("取消消息", AuthorizationPermissions.Messages.Cancel, "消息管理", "取消正在发送的消息"),
            ("查看模板", AuthorizationPermissions.Templates.View, "消息模板", "查看消息模板列表"),
            ("创建模板", AuthorizationPermissions.Templates.Create, "消息模板", "创建新的消息模板"),
            ("编辑模板", AuthorizationPermissions.Templates.Update, "消息模板", "编辑消息模板"),
            ("删除模板", AuthorizationPermissions.Templates.Delete, "消息模板", "删除消息模板"),
        };

        foreach (var (name, code, category, description) in permissions)
        {
            var existing = await permissionGrain.GetPermissionByCodeAsync(code, cancellationToken);
            if (existing is not null)
            {
                logger.LogDebug("权限 {Code} 已存在，跳过", code);
                continue;
            }

            await permissionGrain.CreatePermissionAsync(new PermissionDataDto
            {
                Name = name,
                Code = code,
                Category = category,
                Description = description,
                Type = 3,
                IsActive = true
            }, cancellationToken);
            logger.LogDebug("创建权限: {Name} ({Code})", name, code);
        }
    }

    private async Task<Guid> CreateMessageAdminRoleAsync(
        IPermissionGrain permissionGrain,
        IRoleGrain roleGrain,
        CancellationToken cancellationToken)
    {
        var existingRole = await roleGrain.GetRoleByNameAsync("MessageAdmin", cancellationToken);
        if (existingRole is not null)
        {
            logger.LogDebug("MessageAdmin角色已存在，跳过创建");
            await AssignPermissionsToRoleAsync(permissionGrain, roleGrain, existingRole.Id, cancellationToken);
            return existingRole.Id;
        }

        var newRole = await roleGrain.CreateRoleAsync(new CreateOrUpdateRoleInputDto
        {
            Name = "MessageAdmin",
            Description = "消息管理员角色，拥有消息管理权限",
            IsDefault = false,
            Priority = 100,
            IsActive = true
        }, cancellationToken);

        logger.LogInformation("创建MessageAdmin角色成功: {RoleId}", newRole.Id);
        await AssignPermissionsToRoleAsync(permissionGrain, roleGrain, newRole.Id, cancellationToken);
        return newRole.Id;
    }

    private async Task AssignMessageAdminRoleToAdminUserAsync(
        IUserGrain userGrain,
        IRoleGrain roleGrain,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var adminUser = await userGrain.GetUserByAccountAsync("admin", cancellationToken);
        if (adminUser is null)
        {
            logger.LogWarning("Admin账号不存在，无法分配MessageAdmin角色");
            return;
        }

        var existingRoles = await userGrain.GetUserRolesAsync(adminUser.Id, cancellationToken);
        if (existingRoles.Any(r => r.Id == roleId))
        {
            logger.LogDebug("Admin账号已拥有MessageAdmin角色，跳过分配");
            return;
        }
        var roleIds = existingRoles.Select(r => r.Id).ToList();
        roleIds.Add(roleId);

        var assigned = await userGrain.AssignRolesAsync(adminUser.Id, roleIds, cancellationToken);
        if (assigned)
        {
            logger.LogInformation("已为Admin账号分配MessageAdmin角色");
        }
        else
        {
            logger.LogWarning("为Admin账号分配MessageAdmin角色失败");
        }
    }

    private async Task AssignPermissionsToRoleAsync(
        IPermissionGrain permissionGrain,
        IRoleGrain roleGrain,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var messagePermissions = await permissionGrain.GetPermissionsByCategoryAsync("消息管理", cancellationToken);
        var templatePermissions = await permissionGrain.GetPermissionsByCategoryAsync("消息模板", cancellationToken);
        
        var allPermissions = messagePermissions.Concat(templatePermissions).ToList();
        var permissionIds = allPermissions.Select(p => p.Id).ToList();

        if (permissionIds.Count == 0)
        {
            logger.LogWarning("未找到任何消息管理权限，无法为MessageAdmin角色分配权限");
            return;
        }

        var result = await roleGrain.AssignPermissionsAsync(roleId, permissionIds, cancellationToken);
        if (result)
        {
            logger.LogInformation("为MessageAdmin角色分配了 {Count} 个权限", permissionIds.Count);
        }
    }
}
