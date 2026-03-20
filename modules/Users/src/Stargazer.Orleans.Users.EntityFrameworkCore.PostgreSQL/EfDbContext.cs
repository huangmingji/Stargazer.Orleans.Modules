using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Users;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;

public class EfDbContext(DbContextOptions<EfDbContext> options) : DbContext(options)
{
    public DbSet<UserData> UserDatas { get; set; }
    public DbSet<UserRoleData> UserRoles { get; set; }
    public DbSet<RoleData> RoleDatas { get; set; }
    public DbSet<RolePermissionData> RolePermissions { get; set; }
    public DbSet<PermissionData> PermissionDatas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Configure();
        base.OnModelCreating(modelBuilder);
    }
}