using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.WechatManagement.Grains.SeedData;

public class WechatManagementSeedDataInitializer(
    IClusterClient clusterClient,
    ILogger<WechatManagementSeedDataInitializer> logger) : IStartupTask
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("开始初始化WechatManagement模块种子数据...");
        
        var permissionGrain = clusterClient.GetGrain<IPermissionGrain>(0);
        var roleGrain = clusterClient.GetGrain<IRoleGrain>(0);
        var userGrain = clusterClient.GetGrain<IUserGrain>(0);
        
        await SeedPermissionsAsync(permissionGrain, cancellationToken);
        var wechatAdminRoleId = await CreateWechatAdminRoleAsync(permissionGrain, roleGrain, cancellationToken);
        await AssignWechatAdminRoleToAdminUserAsync(userGrain, roleGrain, wechatAdminRoleId, cancellationToken);
        
        logger.LogInformation("WechatManagement模块种子数据初始化完成");
    }

    private async Task SeedPermissionsAsync(IPermissionGrain permissionGrain, CancellationToken cancellationToken)
    {
        var permissions = new List<(string Name, string Code, string Category, string Description)>
        {
            ("查看公众号", AuthorizationPermissions.Accounts.View, "公众号管理", "查看公众号列表"),
            ("创建公众号", AuthorizationPermissions.Accounts.Create, "公众号管理", "创建公众号配置"),
            ("更新公众号", AuthorizationPermissions.Accounts.Update, "公众号管理", "更新公众号配置"),
            ("删除公众号", AuthorizationPermissions.Accounts.Delete, "公众号管理", "删除公众号配置"),
            ("查看粉丝", AuthorizationPermissions.Fans.View, "粉丝管理", "查看粉丝列表"),
            ("更新粉丝", AuthorizationPermissions.Fans.Update, "粉丝管理", "更新粉丝信息"),
            ("标签粉丝", AuthorizationPermissions.Fans.Tag, "粉丝管理", "为粉丝打标签"),
            ("查看分组", AuthorizationPermissions.Groups.View, "分组管理", "查看粉丝分组"),
            ("创建分组", AuthorizationPermissions.Groups.Create, "分组管理", "创建粉丝分组"),
            ("更新分组", AuthorizationPermissions.Groups.Update, "分组管理", "更新粉丝分组"),
            ("删除分组", AuthorizationPermissions.Groups.Delete, "分组管理", "删除粉丝分组"),
            ("查看标签", AuthorizationPermissions.Tags.View, "标签管理", "查看粉丝标签"),
            ("创建标签", AuthorizationPermissions.Tags.Create, "标签管理", "创建粉丝标签"),
            ("更新标签", AuthorizationPermissions.Tags.Update, "标签管理", "更新粉丝标签"),
            ("删除标签", AuthorizationPermissions.Tags.Delete, "标签管理", "删除粉丝标签"),
            ("发送模板消息", AuthorizationPermissions.Messages.SendTemplate, "消息推送", "发送模板消息"),
            ("发送客服消息", AuthorizationPermissions.Messages.SendCustom, "消息推送", "发送客服消息"),
            ("发送群发消息", AuthorizationPermissions.Messages.SendMass, "消息推送", "发送群发消息"),
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

    private async Task<Guid> CreateWechatAdminRoleAsync(
        IPermissionGrain permissionGrain,
        IRoleGrain roleGrain,
        CancellationToken cancellationToken)
    {
        var existingRole = await roleGrain.GetRoleByNameAsync("WechatAdmin", cancellationToken);
        if (existingRole is not null)
        {
            logger.LogDebug("WechatAdmin角色已存在，跳过创建");
            await AssignPermissionsToRoleAsync(permissionGrain, roleGrain, existingRole.Id, cancellationToken);
            return existingRole.Id;
        }

        var newRole = await roleGrain.CreateRoleAsync(new CreateOrUpdateRoleInputDto
        {
            Name = "WechatAdmin",
            Description = "微信管理员角色，拥有微信管理权限",
            IsDefault = false,
            Priority = 100,
            IsActive = true
        }, cancellationToken);

        logger.LogInformation("创建WechatAdmin角色成功: {RoleId}", newRole.Id);
        await AssignPermissionsToRoleAsync(permissionGrain, roleGrain, newRole.Id, cancellationToken);
        return newRole.Id;
    }

    private async Task AssignWechatAdminRoleToAdminUserAsync(
        IUserGrain userGrain,
        IRoleGrain roleGrain,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var adminUser = await userGrain.GetUserByAccountAsync("admin", cancellationToken);
        if (adminUser is null)
        {
            logger.LogWarning("Admin账号不存在，无法分配WechatAdmin角色");
            return;
        }

        var existingRoles = await userGrain.GetUserRolesAsync(adminUser.Id, cancellationToken);
        if (existingRoles.Any(r => r.Id == roleId))
        {
            logger.LogDebug("Admin账号已拥有WechatAdmin角色，跳过分配");
            return;
        }
        var roleIds = existingRoles.Select(r => r.Id).ToList();
        roleIds.Add(roleId);

        var assigned = await userGrain.AssignRolesAsync(adminUser.Id, roleIds, cancellationToken);
        if (assigned)
        {
            logger.LogInformation("已为Admin账号分配WechatAdmin角色");
        }
        else
        {
            logger.LogWarning("为Admin账号分配WechatAdmin角色失败");
        }
    }

    private async Task AssignPermissionsToRoleAsync(
        IPermissionGrain permissionGrain,
        IRoleGrain roleGrain,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var categories = new[] { "公众号管理", "粉丝管理", "分组管理", "标签管理", "消息推送" };
        
        var allPermissions = new List<PermissionDataDto>();
        foreach (var category in categories)
        {
            var permissions = await permissionGrain.GetPermissionsByCategoryAsync(category, cancellationToken);
            allPermissions.AddRange(permissions);
        }
        
        var permissionIds = allPermissions.Select(p => p.Id).ToList();

        if (permissionIds.Count == 0)
        {
            logger.LogWarning("未找到任何微信管理权限，无法为WechatAdmin角色分配权限");
            return;
        }

        var result = await roleGrain.AssignPermissionsAsync(roleId, permissionIds, cancellationToken);
        if (result)
        {
            logger.LogInformation("为WechatAdmin角色分配了 {Count} 个权限", permissionIds.Count);
        }
    }
}
