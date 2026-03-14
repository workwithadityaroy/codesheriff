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

        builder.Property(m => m.RepositoryId)
            .IsRequired();

        builder.Property(m => m.ClerkUserId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.InvitedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.InviteToken)
            .HasMaxLength(64);

        builder.Property(m => m.AcceptedAt);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasOne(m => m.Repository)
            .WithMany(r => r.Members)
            .HasForeignKey(m => m.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.InviteToken)
            .IsUnique()
            .HasFilter("invite_token IS NOT NULL");

        builder.HasIndex(m => new { m.RepositoryId, m.InvitedEmail })
            .IsUnique();

        builder.HasIndex(m => m.ClerkUserId);
    }
}
