namespace CodeSheriff.Application.Repositories.Queries.GetRepositories;

public sealed record RepositoryDto(
    Guid Id,
    long GitHubId,
    string Owner,
    string Name,
    string FullName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
