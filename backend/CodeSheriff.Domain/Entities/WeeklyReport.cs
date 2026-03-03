using CodeSheriff.Domain.Common;

namespace CodeSheriff.Domain.Entities;

public class WeeklyReport : Entity
{
    public Guid RepositoryId { get; private set; }
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public decimal AverageTechDebtScore { get; private set; }
    public int TotalReviews { get; private set; }
    public int TotalIssues { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public Repository Repository { get; private set; } = null!;

    private WeeklyReport() { }

    public static WeeklyReport Create(
        Guid repositoryId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        decimal averageTechDebtScore,
        int totalReviews,
        int totalIssues,
        string summary)
    {
        return new WeeklyReport
        {
            RepositoryId = repositoryId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            AverageTechDebtScore = averageTechDebtScore,
            TotalReviews = totalReviews,
            TotalIssues = totalIssues,
            Summary = summary,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
