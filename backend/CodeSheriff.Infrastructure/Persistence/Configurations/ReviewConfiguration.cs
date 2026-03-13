using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.PullRequestId)
            .HasColumnName("pull_request_id")
            .IsRequired();

        builder.Property(r => r.TechDebtScore)
            .HasColumnName("tech_debt_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.Summary)
            .HasColumnName("summary")
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.RawAiResponse)
            .HasColumnName("raw_ai_response")
            .IsRequired(false);

        builder.Property(r => r.ProcessingTimeMs)
            .HasColumnName("processing_time_ms")
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(r => r.PullRequestId)
            .HasDatabaseName("ix_reviews_pull_request_id");

        builder.HasMany(r => r.Issues)
            .WithOne(i => i.Review)
            .HasForeignKey(i => i.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
