using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(s => s.ClerkUserId).HasColumnName("clerk_user_id").IsRequired().HasMaxLength(255);
        builder.Property(s => s.AiProvider).HasColumnName("ai_provider").IsRequired().HasMaxLength(50);
        builder.Property(s => s.AiModel).HasColumnName("ai_model").HasMaxLength(100);
        builder.Property(s => s.AiApiKey).HasColumnName("ai_api_key").HasMaxLength(1000);
        builder.Property(s => s.NotificationEmail).HasColumnName("notification_email").HasMaxLength(255);
        builder.Property(s => s.WeeklyReportEnabled).HasColumnName("weekly_report_enabled").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(s => s.ClerkUserId).IsUnique().HasDatabaseName("ix_user_settings_clerk_user_id");
    }
}
