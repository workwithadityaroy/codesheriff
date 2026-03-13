using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(CodeSheriffDbContext context) : base(context) { }

    public async Task<Review?> GetByPullRequestIdAsync(
        Guid pullRequestId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(r => r.PullRequestId == pullRequestId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Review?> GetWithIssuesByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

    public async Task<Review?> GetLatestWithIssuesByPullRequestIdAsync(
        Guid pullRequestId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(r => r.Issues)
            .Where(r => r.PullRequestId == pullRequestId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
