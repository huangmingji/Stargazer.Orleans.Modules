using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.MessageManagement.Domain;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

public static class DbContextModelCreatingExtensions
{
    public static void Configure(this ModelBuilder modelBuilder)
    {
        ConfigureMessageRecords(modelBuilder);
        ConfigureMessageTemplates(modelBuilder);
        ConfigureProviderConfigs(modelBuilder);
    }

    private static void ConfigureMessageRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageRecord>(entity =>
        {
            entity.ToTable("msg_records");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Channel).HasColumnName("channel").IsRequired();
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.TemplateCode).HasColumnName("template_code").HasMaxLength(100);
            entity.Property(e => e.Receiver).HasColumnName("receiver").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(500);
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.Variables).HasColumnName("variables").HasColumnType("jsonb");
            entity.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.ExternalId).HasColumnName("external_id").HasMaxLength(200);
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id").HasMaxLength(200);
            entity.Property(e => e.BusinessType).HasColumnName("business_type").HasMaxLength(100);
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => e.Channel).HasDatabaseName("idx_msg_records_channel");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_msg_records_status");
            entity.HasIndex(e => e.Receiver).HasDatabaseName("idx_msg_records_receiver");
            entity.HasIndex(e => e.ScheduledAt).HasDatabaseName("idx_msg_records_scheduled_at")
                .HasFilter("scheduled_at IS NOT NULL");
            entity.HasIndex(e => e.CreationTime).HasDatabaseName("idx_msg_records_creation_time")
                .IsDescending();
        });
    }

    private static void ConfigureMessageTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageTemplate>(entity =>
        {
            entity.ToTable("msg_templates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Channel).HasColumnName("channel").IsRequired();
            entity.Property(e => e.SubjectTemplate).HasColumnName("subject_template").HasMaxLength(500);
            entity.Property(e => e.ContentTemplate).HasColumnName("content_template").IsRequired();
            entity.Property(e => e.Variables).HasColumnName("variables").HasColumnType("jsonb");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.DefaultProvider).HasColumnName("default_provider").HasMaxLength(50);
            entity.Property(e => e.Tags).HasColumnName("tags").HasMaxLength(500);
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => new { e.Code, e.Channel }).IsUnique();
            entity.HasIndex(e => e.Channel).HasDatabaseName("idx_msg_templates_channel");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_msg_templates_is_active");
        });
    }

    private static void ConfigureProviderConfigs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProviderConfig>(entity =>
        {
            entity.ToTable("msg_provider_configs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Channel).HasColumnName("channel").IsRequired();
            entity.Property(e => e.ConfigJson).HasColumnName("config_json").IsRequired();
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.MaxQps).HasColumnName("max_qps");
            entity.Property(e => e.CurrentQps).HasColumnName("current_qps");
            entity.Property(e => e.IsHealthy).HasColumnName("is_healthy");
            entity.Property(e => e.LastCheckAt).HasColumnName("last_check_at");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time").IsRequired();
            entity.Property(e => e.LastModifierId).HasColumnName("last_modifier_id");
            entity.Property(e => e.LastModifyTime).HasColumnName("last_modify_time").IsRequired();

            entity.HasIndex(e => e.Channel).HasDatabaseName("idx_msg_provider_configs_channel");
            entity.HasIndex(e => e.IsEnabled).HasDatabaseName("idx_msg_provider_configs_is_enabled");
        });
    }
}
