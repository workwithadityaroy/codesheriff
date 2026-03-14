using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSheriff.Infrastructure.Persistence.Configurations;

internal sealed class RepositoryMemberConfiguration : IEntityTypeConfiguration<RepositoryMember>
{
    public void Configure(EntityTypeBuilder<RepositoryMember> builder)
    {
        builder.ToTable("repository_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(m => m.ClerkUserId)
            .HasColumnName("clerk_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.InvitedEmail)
            .HasColumnName("invited_email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.InviteToken)
            .HasColumnName("invite_token")
            .HasMaxLength(64);

        builder.Property(m => m.AcceptedAt)
            .HasColumnName("accepted_at");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(m => m.Repository)
            .WithMany(r => r.Members)
            .HasForeignKey(m => m.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.InviteToken)
            .IsUnique()
            .HasFilter("invite_token IS NOT NULL")
            .HasDatabaseName("ix_repository_members_invite_token");

        builder.HasIndex(m => new { m.RepositoryId, m.InvitedEmail })
            .IsUnique()
            .HasDatabaseName("ix_repository_members_repository_id_invited_email");

        builder.HasIndex(m => m.ClerkUserId)
            .HasDatabaseName("ix_repository_members_clerk_user_id");
    }
}
