using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.WechatManagement.Domain.Accounts;
using Stargazer.Orleans.WechatManagement.Domain.Messages;
using Stargazer.Orleans.WechatManagement.Domain.Users;

namespace Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;

public class EfDbContext(DbContextOptions<EfDbContext> options) : DbContext(options)
{
    public DbSet<WechatAccount> WechatAccounts { get; set; }
    public DbSet<WechatUser> WechatUsers { get; set; }
    public DbSet<WechatUserGroup> WechatUserGroups { get; set; }
    public DbSet<WechatUserTag> WechatUserTags { get; set; }
    public DbSet<WechatMessageLog> WechatMessageLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Configure();
        base.OnModelCreating(modelBuilder);
    }
}
