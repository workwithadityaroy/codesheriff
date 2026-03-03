using CodeSheriff.Domain.Interfaces;
using CodeSheriff.Infrastructure.Persistence.Repositories;

namespace CodeSheriff.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly CodeSheriffDbContext _context;
    private bool _disposed;

    public IRepositoryRepository Repositories { get; }
    public IPullRequestRepository PullRequests { get; }
    public IReviewRepository Reviews { get; }
    public IWeeklyReportRepository WeeklyReports { get; }

    public UnitOfWork(CodeSheriffDbContext context)
    {
        _context = context;
        Repositories = new RepositoryRepository(context);
        PullRequests = new PullRequestRepository(context);
        Reviews = new ReviewRepository(context);
        WeeklyReports = new WeeklyReportRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
