using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.WechatManagement.Domain;
using Stargazer.Orleans.WechatManagement.Domain.Accounts;
using Stargazer.Orleans.WechatManagement.Domain.Messages;
using Stargazer.Orleans.WechatManagement.Domain.Users;

namespace Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;

public static class DbContextModelCreatingExtensions
{
    public static void Configure(this ModelBuilder builder)
    {
        ConfigureWechatAccount(builder);
        ConfigureWechatUser(builder);
        ConfigureWechatUserGroup(builder);
        ConfigureWechatUserTag(builder);
        ConfigureWechatMessageLog(builder);
        ConfigureWechatUserBinding(builder);
    }

    private static void ConfigureWechatAccount(ModelBuilder builder)
    {
        builder.Entity<WechatAccount>(entity =>
        {
            entity.ToTable("wechat_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AppId).HasColumnName("app_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AppSecret).HasColumnName("app_secret").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(100);
            entity.Property(e => e.EncodingAESKey).HasColumnName("encoding_aes_key").HasMaxLength(100);
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.AccessToken).HasColumnName("access_token").HasMaxLength(500);
            entity.Property(e => e.AccessTokenExpiry).HasColumnName("access_token_expiry");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time");

            entity.HasIndex(e => e.AppId).IsUnique();
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.IsActive);
        });
    }

    private static void ConfigureWechatUser(ModelBuilder builder)
    {
        builder.Entity<WechatUser>(entity =>
        {
            entity.ToTable("wechat_users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OpenId).HasColumnName("open_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.UnionId).HasColumnName("union_id").HasMaxLength(100);
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Nickname).HasColumnName("nickname").HasMaxLength(200);
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Province).HasColumnName("province").HasMaxLength(50);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(50);
            entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(50);
            entity.Property(e => e.HeadimgUrl).HasColumnName("headimg_url").HasMaxLength(500);
            entity.Property(e => e.SubscribeTime).HasColumnName("subscribe_time");
            entity.Property(e => e.UnSubscribeTime).HasColumnName("unsubscribe_time");
            entity.Property(e => e.Remark).HasColumnName("remark").HasMaxLength(200);
            entity.Property(e => e.SubscribeStatus).HasColumnName("subscribe_status").HasDefaultValue(1);
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time");

            entity.HasIndex(e => e.OpenId).IsUnique();
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.SubscribeStatus);
            entity.HasIndex(e => new { e.AccountId, e.OpenId }).IsUnique();

            entity.HasOne(e => e.Account)
                .WithMany(a => a.Users)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureWechatUserGroup(ModelBuilder builder)
    {
        builder.Entity<WechatUserGroup>(entity =>
        {
            entity.ToTable("wechat_user_groups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.WechatGroupId).HasColumnName("wechat_group_id");
            entity.Property(e => e.UserCount).HasColumnName("user_count").HasDefaultValue(0);
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time");

            entity.HasIndex(e => new { e.AccountId, e.Name }).IsUnique();

            entity.HasOne(e => e.Account)
                .WithMany(a => a.UserGroups)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWechatUserTag(ModelBuilder builder)
    {
        builder.Entity<WechatUserTag>(entity =>
        {
            entity.ToTable("wechat_user_tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.WechatTagId).HasColumnName("wechat_tag_id");
            entity.Property(e => e.UserCount).HasColumnName("user_count").HasDefaultValue(0);
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time");

            entity.HasIndex(e => new { e.AccountId, e.Name }).IsUnique();

            entity.HasOne(e => e.Account)
                .WithMany(a => a.UserTags)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWechatMessageLog(ModelBuilder builder)
    {
        builder.Entity<WechatMessageLog>(entity =>
        {
            entity.ToTable("wechat_message_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.OpenId).HasColumnName("open_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TemplateId).HasColumnName("template_id").HasMaxLength(100);
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue(0);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(e => e.SendTime).HasColumnName("send_time");
            entity.Property(e => e.CompleteTime).HasColumnName("complete_time");
            entity.Property(e => e.MsgId).HasColumnName("msg_id").HasMaxLength(100);
            entity.Property(e => e.ExtraData).HasColumnName("extra_data");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time");

            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OpenId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreationTime);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWechatUserBinding(ModelBuilder builder)
    {
        builder.Entity<WechatUserBinding>(entity =>
        {
            entity.ToTable("wechat_user_bindings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WechatUserId).HasColumnName("wechat_user_id").IsRequired();
            entity.Property(e => e.LocalUserId).HasColumnName("local_user_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.OpenId).HasColumnName("open_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.BindingTime).HasColumnName("binding_time");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OpenId);
            entity.HasIndex(e => e.LocalUserId);
            entity.HasIndex(e => new { e.AccountId, e.OpenId }).IsUnique();
        });
    }
}
