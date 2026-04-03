using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stargazer.Common;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.Grains.Abstractions;

namespace Stargazer.Orleans.Users.Grains.Grains;

public class UsersSeedDataInitializer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<UsersSeedDataInitializer> logger) : IStartupTask
{
    private const string AdminAccount = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string AdminName = "系统管理员";

    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("开始初始化Users模块种子数据...");
        
        using var scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<UserData, Guid>>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRepository<RoleData, Guid>>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IRepository<PermissionData, Guid>>();
        var rolePermissionRepository = scope.ServiceProvider.GetRequiredService<IRepository<RolePermissionData, Guid>>();
        var userRoleRepository = scope.ServiceProvider.GetRequiredService<IRepository<UserRoleData, Guid>>();
        
        await SeedPermissionsAsync(permissionRepository, cancellationToken);
        var adminRole = await SeedAdminRoleAsync(roleRepository, permissionRepository, rolePermissionRepository, cancellationToken);
        await SeedAdminUserAsync(userRepository, roleRepository, userRoleRepository, adminRole, cancellationToken);
        
        logger.LogInformation("Users模块种子数据初始化完成");
    }

    private async Task SeedPermissionsAsync(
        IRepository<PermissionData, Guid> permissionRepository,
        CancellationToken cancellationToken)
    {
        var permissionCodes = new List<(string Name, string Code, string Category, string Description)>
        {
            ("用户管理", "users.view", "用户管理", "查看用户列表"),
            ("创建用户", "users.create", "用户管理", "创建新用户"),
            ("编辑用户", "users.edit", "用户管理", "编辑用户信息"),
            ("删除用户", "users.delete", "用户管理", "删除用户"),
            ("分配角色", "users.assign_role", "用户管理", "为用户分配角色"),
            
            ("角色管理", "roles.view", "角色管理", "查看角色列表"),
            ("创建角色", "roles.create", "角色管理", "创建新角色"),
            ("编辑角色", "roles.edit", "角色管理", "编辑角色信息"),
            ("删除角色", "roles.delete", "角色管理", "删除角色"),
            ("分配权限", "roles.assign_permission", "角色管理", "为角色分配权限"),
            
            ("权限管理", "permissions.view", "权限管理", "查看权限列表"),
            ("创建权限", "permissions.create", "权限管理", "创建新权限"),
            ("编辑权限", "permissions.edit", "权限管理", "编辑权限信息"),
            ("删除权限", "permissions.delete", "权限管理", "删除权限"),
        };

        foreach (var (name, code, category, description) in permissionCodes)
        {
            var existing = await permissionRepository.FindAsync(x => x.Code == code, cancellationToken);
            if (existing is not null)
            {
                logger.LogDebug("权限 {Code} 已存在，跳过", code);
                continue;
            }

            var permission = new PermissionData
            {
                Id = new SequentialGuid().Create(),
                Name = name,
                Code = code,
                Category = category,
                Description = description,
                Type = PermissionType.Button,
                IsActive = true,
                CreationTime = DateTime.UtcNow
            };
            await permissionRepository.InsertAsync(permission, cancellationToken);
            logger.LogDebug("创建权限: {Name} ({Code})", name, code);
        }
    }

    private async Task<RoleData> SeedAdminRoleAsync(
        IRepository<RoleData, Guid> roleRepository,
        IRepository<PermissionData, Guid> permissionRepository,
        IRepository<RolePermissionData, Guid> rolePermissionRepository,
        CancellationToken cancellationToken)
    {
        var adminRole = await roleRepository.FindAsync(x => x.Name == "Admin", cancellationToken);
        if (adminRole is not null)
        {
            logger.LogDebug("Admin角色已存在，跳过创建");
            await AssignAllPermissionsToRoleAsync(adminRole.Id, permissionRepository, rolePermissionRepository, cancellationToken);
            return adminRole;
        }

        adminRole = new RoleData
        {
            Id = new SequentialGuid().Create(),
            Name = "Admin",
            Description = "系统管理员角色，拥有所有权限",
            IsDefault = false,
            Priority = 999,
            IsActive = true,
            CreationTime = DateTime.UtcNow
        };
        await roleRepository.InsertAsync(adminRole, cancellationToken);
        logger.LogInformation("创建Admin角色成功: {RoleId}", adminRole.Id);

        await AssignAllPermissionsToRoleAsync(adminRole.Id, permissionRepository, rolePermissionRepository, cancellationToken);
        return adminRole;
    }

    private async Task AssignAllPermissionsToRoleAsync(
        Guid roleId,
        IRepository<PermissionData, Guid> permissionRepository,
        IRepository<RolePermissionData, Guid> rolePermissionRepository,
        CancellationToken cancellationToken)
    {
        var allPermissions = await permissionRepository.FindAllAsync(cancellationToken);
        if (!allPermissions.Any())
        {
            logger.LogWarning("未找到任何权限，无法为Admin角色分配权限");
            return;
        }

        var existingRolePermissions = await rolePermissionRepository.FindListAsync(
            x => x.RoleId == roleId, 
            cancellationToken);
        var existingPermissionIds = existingRolePermissions.Select(x => x.PermissionId).ToHashSet();

        var newPermissions = allPermissions.Where(p => !existingPermissionIds.Contains(p.Id)).ToList();
        if (newPermissions.Any())
        {
            var rolePermissions = newPermissions.Select(p => new RolePermissionData
            {
                Id = new SequentialGuid().Create(),
                RoleId = roleId,
                PermissionId = p.Id,
                CreationTime = DateTime.UtcNow
            }).ToList();

            await rolePermissionRepository.InsertAsync(rolePermissions, cancellationToken);
            logger.LogInformation("为Admin角色分配了 {Count} 个新权限", newPermissions.Count);
        }
        else
        {
            logger.LogDebug("Admin角色已拥有所有权限");
        }
    }

    private async Task SeedAdminUserAsync(
        IRepository<UserData, Guid> userRepository,
        IRepository<RoleData, Guid> roleRepository,
        IRepository<UserRoleData, Guid> userRoleRepository,
        RoleData adminRole,
        CancellationToken cancellationToken)
    {
        var existingAdmin = await userRepository.FindAsync(x => x.Account == AdminAccount, cancellationToken);
        if (existingAdmin is not null)
        {
            logger.LogDebug("Admin用户已存在，跳过创建");
            await AssignRoleToUserAsync(existingAdmin.Id, adminRole.Id, userRoleRepository, cancellationToken);
            return;
        }

        var adminUser = new UserData
        {
            Id = new SequentialGuid().Create(),
            Account = AdminAccount,
            Password = Cryptography.PasswordStorage.CreateHash(AdminPassword, out string secretKey),
            SecretKey = secretKey,
            Name = AdminName,
            IsActive = true,
            CreationTime = DateTime.UtcNow
        };
        await userRepository.InsertAsync(adminUser, cancellationToken);
        logger.LogInformation("创建Admin用户成功: {UserId}, 账号: {Account}, 密码: {Password}", 
            adminUser.Id, AdminAccount, AdminPassword);

        await AssignRoleToUserAsync(adminUser.Id, adminRole.Id, userRoleRepository, cancellationToken);
    }

    private async Task AssignRoleToUserAsync(
        Guid userId,
        Guid roleId,
        IRepository<UserRoleData, Guid> userRoleRepository,
        CancellationToken cancellationToken)
    {
        var existingUserRole = await userRoleRepository.FindAsync(
            x => x.UserId == userId && x.RoleId == roleId, 
            cancellationToken);
        
        if (existingUserRole is not null)
        {
            logger.LogDebug("用户 {UserId} 已拥有Admin角色，跳过", userId);
            return;
        }

        var userRole = new UserRoleData
        {
            Id = new SequentialGuid().Create(),
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            CreationTime = DateTime.UtcNow
        };
        await userRoleRepository.InsertAsync(userRole, cancellationToken);
        logger.LogInformation("为用户 {UserId} 分配Admin角色成功", userId);
    }
}
