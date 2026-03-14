namespace CodeSheriff.Application.Members;

public sealed record MemberDto(
    Guid Id,
    Guid RepositoryId,
    string ClerkUserId,
    string InvitedEmail,
    string Role,
    bool IsAccepted,
    DateTimeOffset CreatedAt);
