using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Entities;

public class ReviewIssue : Entity
{
    public Guid ReviewId { get; private set; }
    public IssueSeverity Severity { get; private set; }
    public IssueCategory Category { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public int? LineNumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Suggestion { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public Review Review { get; private set; } = null!;

    private ReviewIssue() { }

    public static ReviewIssue Create(
        Guid reviewId,
        IssueSeverity severity,
        IssueCategory category,
        string filePath,
        int? lineNumber,
        string description,
        string suggestion)
    {
        return new ReviewIssue
        {
            ReviewId = reviewId,
            Severity = severity,
            Category = category,
            FilePath = filePath,
            LineNumber = lineNumber,
            Description = description,
            Suggestion = suggestion,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
