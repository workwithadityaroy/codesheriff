using CodeSheriff.Domain.Entities;
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
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
}
