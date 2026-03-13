using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class WeeklyReportRepository : BaseRepository<WeeklyReport>, IWeeklyReportRepository
{
    public WeeklyReportRepository(CodeSheriffDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WeeklyReport>> GetByRepositoryIdAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(wr => wr.RepositoryId == repositoryId)
            .OrderByDescending(wr => wr.PeriodStart)
            .ToListAsync(cancellationToken);

    public async Task<WeeklyReport?> GetLatestByRepositoryIdAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(wr => wr.RepositoryId == repositoryId)
            .OrderByDescending(wr => wr.PeriodStart)
            .FirstOrDefaultAsync(cancellationToken);
}
