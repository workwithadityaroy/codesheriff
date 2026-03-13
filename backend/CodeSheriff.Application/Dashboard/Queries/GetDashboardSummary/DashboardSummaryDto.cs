namespace CodeSheriff.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record DashboardSummaryDto(
    int TotalRepositories,
    int TotalPrsReviewed,
    decimal AverageTechDebtScore,
    int CriticalIssueCount
);
