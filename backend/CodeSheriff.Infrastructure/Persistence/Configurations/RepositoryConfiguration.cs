using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainRepository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class RepositoryConfiguration : IEntityTypeConfiguration<DomainRepository>
{
    public void Configure(EntityTypeBuilder<DomainRepository> builder)
    {
        builder.ToTable("repositories");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.GitHubId)
            .HasColumnName("github_id")
            .IsRequired();

        builder.Property(r => r.Owner)
            .HasColumnName("owner")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(511);

        builder.Property(r => r.InstallationId)
            .HasColumnName("installation_id")
            .IsRequired();

        builder.Property(r => r.ClerkUserId)
            .HasColumnName("clerk_user_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(r => r.GitHubId)
            .IsUnique()
            .HasDatabaseName("ix_repositories_github_id");

        builder.HasIndex(r => r.ClerkUserId)
            .HasDatabaseName("ix_repositories_clerk_user_id");

        builder.HasIndex(r => r.FullName)
            .IsUnique()
            .HasDatabaseName("ix_repositories_full_name");

        builder.HasMany(r => r.PullRequests)
            .WithOne(pr => pr.Repository)
            .HasForeignKey(pr => pr.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.WeeklyReports)
            .WithOne(wr => wr.Repository)
            .HasForeignKey(wr => wr.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
