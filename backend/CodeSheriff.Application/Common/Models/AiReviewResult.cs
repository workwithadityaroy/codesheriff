using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Application.Common.Models;

public sealed record AiReviewResult(
    decimal TechDebtScore,
    string Summary,
    string RawResponse,
    IReadOnlyList<AiReviewIssue> Issues);

public sealed record AiReviewIssue(
    IssueSeverity Severity,
    IssueCategory Category,
    string FilePath,
    int? LineNumber,
    string Description,
    string Suggestion);
