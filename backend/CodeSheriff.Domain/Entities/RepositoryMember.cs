using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Entities;

public class RepositoryMember : Entity
{
    public Guid RepositoryId { get; private set; }
    public string ClerkUserId { get; private set; } = string.Empty;
    public string InvitedEmail { get; private set; } = string.Empty;
    public MemberRole Role { get; private set; }
    public string? InviteToken { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Repository Repository { get; private set; } = null!;

    private RepositoryMember() { }

    public static RepositoryMember CreateInvite(Guid repositoryId, string invitedEmail, MemberRole role)
        => new()
        {
            RepositoryId = repositoryId,
            InvitedEmail = invitedEmail.ToLowerInvariant(),
            Role = role,
            InviteToken = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow,
        };

    public void Accept(string clerkUserId)
    {
        ClerkUserId = clerkUserId;
        InviteToken = null;
        AcceptedAt = DateTimeOffset.UtcNow;
    }
}
