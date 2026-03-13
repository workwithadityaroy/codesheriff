using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Entities;

public class Review : Entity
{
    public Guid PullRequestId { get; private set; }
    public decimal TechDebtScore { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public ReviewStatus Status { get; private set; }
    public string? RawAiResponse { get; private set; }
    public int? ProcessingTimeMs { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public PullRequest PullRequest { get; private set; } = null!;
    public ICollection<ReviewIssue> Issues { get; private set; } = new List<ReviewIssue>();

    private Review() { }

    public static Review Create(Guid pullRequestId)
    {
        return new Review
        {
            PullRequestId = pullRequestId,
            TechDebtScore = 0,
            Summary = string.Empty,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        Status = ReviewStatus.Processing;
    }

    public void Complete(
        decimal techDebtScore,
        string summary,
        string rawAiResponse,
        int processingTimeMs)
    {
        TechDebtScore = techDebtScore;
        Summary = summary;
        RawAiResponse = rawAiResponse;
        ProcessingTimeMs = processingTimeMs;
        Status = ReviewStatus.Completed;
    }

    public void MarkAsFailed()
    {
        Status = ReviewStatus.Failed;
    }
}
