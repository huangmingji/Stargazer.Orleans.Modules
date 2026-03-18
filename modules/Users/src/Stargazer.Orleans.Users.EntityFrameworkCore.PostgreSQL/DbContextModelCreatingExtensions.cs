using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Users;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL
{
    public static class DbContextModelCreatingExtensions
    {
        public static void Configure(this ModelBuilder builder)
        {
            builder.Entity<UserData>(b =>
            {
                b.ToTable(nameof(UserData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => x.Account).IsUnique();
                b.HasIndex(x => x.Email).IsUnique();
                b.HasIndex(x => x.PhoneNumber).IsUnique();
            });
            
            builder.Entity<RoleData>(b =>
            {
                b.ToTable(nameof(RoleData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => x.Name).IsUnique();
            });
            
            builder.Entity<PermissionData>(b =>
            {
                b.ToTable(nameof(PermissionData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => x.Code).IsUnique();
            });
            
            builder.Entity<UserRoleData>(b =>
            {
                b.ToTable(nameof(UserRoleData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            });
            
            builder.Entity<RoleData>()
                .HasMany(x => x.Permissions)
                .WithMany(x => x.Roles)
                .UsingEntity(j => j.ToTable("RolePermissions"));
            
            builder.Entity<UserData>()
                .HasMany(x => x.UserRoles)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<RoleData>()
                .HasMany(x => x.UserRoles)
                .WithOne()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}