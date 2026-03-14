using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class PullRequestRepository : BaseRepository<PullRequest>, IPullRequestRepository
{
    public PullRequestRepository(CodeSheriffDbContext context) : base(context) { }

    public async Task<PullRequest?> GetByGitHubPrNumberAsync(
        Guid repositoryId,
        int prNumber,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                pr => pr.RepositoryId == repositoryId && pr.GitHubPrNumber == prNumber,
                cancellationToken);

    public async Task<IReadOnlyList<PullRequest>> GetByRepositoryIdAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(pr => pr.RepositoryId == repositoryId)
            .OrderByDescending(pr => pr.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<PullRequest> Items, int TotalCount)> GetPagedByRepositoryIdAsync(
        Guid repositoryId,
        int page,
        int pageSize,
        string? statusFilter,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(pr => pr.RepositoryId == repositoryId);

        if (!string.IsNullOrEmpty(statusFilter)
            && Enum.TryParse<PullRequestStatus>(statusFilter, ignoreCase: true, out var status))
        {
            query = query.Where(pr => pr.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(pr => pr.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
