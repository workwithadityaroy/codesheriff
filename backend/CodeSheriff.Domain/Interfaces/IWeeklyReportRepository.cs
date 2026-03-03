using CodeSheriff.Domain.Entities;

namespace CodeSheriff.Domain.Interfaces;

public interface IWeeklyReportRepository : IRepository<WeeklyReport>
{
    Task<IReadOnlyList<WeeklyReport>> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<WeeklyReport?> GetLatestByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);
}
