namespace CodeSheriff.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWeeklyReportAsync(
        string toEmail,
        string displayName,
        WeeklyReportData data,
        CancellationToken cancellationToken = default);
}

public sealed record WeeklyReportData(
    int TotalReviewed,
    decimal AverageTechDebtScore,
    int CriticalIssueCount,
    IReadOnlyList<RepoReportItem> Repos);

public sealed record RepoReportItem(
    string FullName,
    int PrsReviewed,
    decimal AvgDebtScore);
