using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class PullRequestConfiguration : IEntityTypeConfiguration<PullRequest>
{
    public void Configure(EntityTypeBuilder<PullRequest> builder)
    {
        builder.ToTable("pull_requests");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pr => pr.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(pr => pr.GitHubPrNumber)
            .HasColumnName("github_pr_number")
            .IsRequired();

        builder.Property(pr => pr.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pr => pr.HeadBranch)
            .HasColumnName("head_branch")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pr => pr.BaseBranch)
            .HasColumnName("base_branch")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pr => pr.AuthorLogin)
            .HasColumnName("author_login")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pr => pr.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(pr => pr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pr => pr.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(pr => new { pr.RepositoryId, pr.GitHubPrNumber })
            .IsUnique()
            .HasDatabaseName("ix_pull_requests_repository_pr_number");

        builder.HasMany(pr => pr.Reviews)
            .WithOne(r => r.PullRequest)
            .HasForeignKey(r => r.PullRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
