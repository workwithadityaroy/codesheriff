namespace CodeSheriff.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepositoryRepository Repositories { get; }
    IPullRequestRepository PullRequests { get; }
    IReviewRepository Reviews { get; }
    IWeeklyReportRepository WeeklyReports { get; }
    IUserSettingsRepository UserSettings { get; }
    IRepositoryMemberRepository Members { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
