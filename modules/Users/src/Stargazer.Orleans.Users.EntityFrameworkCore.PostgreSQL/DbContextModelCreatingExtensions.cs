using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Users;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;

public static class DbContextModelCreatingExtensions
{
    public static void Configure(this ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder);
        ConfigureRoles(modelBuilder);
        ConfigurePermissions(modelBuilder);
        ConfigureUserRoles(modelBuilder);
        ConfigureRolePermissions(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserData>(entity =>
        {
            entity.ToTable("sys_users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Account).HasColumnName("account").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(500).IsRequired();
            entity.Property(e => e.SecretKey).HasColumnName("secret_key").HasMaxLength(500);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.Avatar).HasColumnName("avatar").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => e.Account).IsUnique().HasDatabaseName("idx_sys_users_account");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("idx_sys_users_email");
            entity.HasIndex(e => e.PhoneNumber).IsUnique().HasDatabaseName("idx_sys_users_phone_number");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_sys_users_is_active");
            entity.HasIndex(e => e.CreationTime).HasDatabaseName("idx_sys_users_creation_time");

            entity.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleData>(entity =>
        {
            entity.ToTable("sys_roles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("idx_sys_roles_name");
            entity.HasIndex(e => e.IsDefault).HasDatabaseName("idx_sys_roles_is_default");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_sys_roles_is_active");
            entity.HasIndex(e => e.Priority).HasDatabaseName("idx_sys_roles_priority");

            entity.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionData>(entity =>
        {
            entity.ToTable("sys_permissions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => e.Code).IsUnique().HasDatabaseName("idx_sys_permissions_code");
            entity.HasIndex(e => e.Category).HasDatabaseName("idx_sys_permissions_category");
            entity.HasIndex(e => e.Type).HasDatabaseName("idx_sys_permissions_type");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_sys_permissions_is_active");
        });
    }

    private static void ConfigureUserRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRoleData>(entity =>
        {
            entity.ToTable("sys_user_roles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ExpireTime).HasColumnName("expire_time");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => new { e.UserId, e.RoleId })
                .IsUnique()
                .HasDatabaseName("idx_sys_user_roles_user_role");

            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_sys_user_roles_user_id");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("idx_sys_user_roles_role_id");
            entity.HasIndex(e => e.ExpireTime)
                .HasDatabaseName("idx_sys_user_roles_expire_time")
                .HasFilter("expire_time IS NOT NULL");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_sys_user_roles_is_active");
        });
    }

    private static void ConfigureRolePermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermissionData>(entity =>
        {
            entity.ToTable("sys_role_permissions");

            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasIndex(e => e.RoleId).HasDatabaseName("idx_sys_role_permissions_role_id");
            entity.HasIndex(e => e.PermissionId).HasDatabaseName("idx_sys_role_permissions_permission_id");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
