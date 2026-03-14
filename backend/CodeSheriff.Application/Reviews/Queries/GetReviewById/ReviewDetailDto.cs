namespace CodeSheriff.Application.Reviews.Queries.GetReviewById;

public sealed record ReviewDetailDto(
    Guid Id,
    Guid PullRequestId,
    int GitHubPrNumber,
    string PullRequestTitle,
    string RepositoryFullName,
    decimal TechDebtScore,
    string Summary,
    string Status,
    int? ProcessingTimeMs,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ReviewIssueDto> Issues
);

public sealed record ReviewIssueDto(
    Guid Id,
    string Severity,
    string Category,
    string FilePath,
    int? LineNumber,
    string Description,
    string Suggestion
);
