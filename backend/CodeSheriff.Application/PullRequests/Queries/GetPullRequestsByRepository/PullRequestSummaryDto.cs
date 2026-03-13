namespace CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;

public sealed record PullRequestSummaryDto(
    Guid Id,
    int GitHubPrNumber,
    string Title,
    string HeadBranch,
    string BaseBranch,
    string AuthorLogin,
    string Status,
    Guid? LatestReviewId,
    decimal? LatestTechDebtScore,
    string? LatestReviewStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
