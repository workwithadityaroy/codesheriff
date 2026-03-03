using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class ReviewIssueConfiguration : IEntityTypeConfiguration<ReviewIssue>
{
    public void Configure(EntityTypeBuilder<ReviewIssue> builder)
    {
        builder.ToTable("review_issues");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.ReviewId)
            .HasColumnName("review_id")
            .IsRequired();

        builder.Property(i => i.Severity)
            .HasColumnName("severity")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Category)
            .HasColumnName("category")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.FilePath)
            .HasColumnName("file_path")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.LineNumber)
            .HasColumnName("line_number")
            .IsRequired(false);

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.Suggestion)
            .HasColumnName("suggestion")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(i => i.ReviewId)
            .HasDatabaseName("ix_review_issues_review_id");

        builder.HasIndex(i => i.Severity)
            .HasDatabaseName("ix_review_issues_severity");
    }
}
