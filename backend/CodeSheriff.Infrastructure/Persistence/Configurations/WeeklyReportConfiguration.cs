using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class WeeklyReportConfiguration : IEntityTypeConfiguration<WeeklyReport>
{
    public void Configure(EntityTypeBuilder<WeeklyReport> builder)
    {
        builder.ToTable("weekly_reports");

        builder.HasKey(wr => wr.Id);

        builder.Property(wr => wr.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(wr => wr.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(wr => wr.PeriodStart)
            .HasColumnName("period_start")
            .IsRequired();

        builder.Property(wr => wr.PeriodEnd)
            .HasColumnName("period_end")
            .IsRequired();

        builder.Property(wr => wr.AverageTechDebtScore)
            .HasColumnName("average_tech_debt_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(wr => wr.TotalReviews)
            .HasColumnName("total_reviews")
            .IsRequired();

        builder.Property(wr => wr.TotalIssues)
            .HasColumnName("total_issues")
            .IsRequired();

        builder.Property(wr => wr.Summary)
            .HasColumnName("summary")
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(wr => wr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(wr => wr.RepositoryId)
            .HasDatabaseName("ix_weekly_reports_repository_id");

        builder.HasIndex(wr => new { wr.RepositoryId, wr.PeriodStart })
            .HasDatabaseName("ix_weekly_reports_repository_period");
    }
}
