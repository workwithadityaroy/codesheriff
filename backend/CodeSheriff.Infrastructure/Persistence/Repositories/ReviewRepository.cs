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
            .FirstOrDefaultAsync(r => r.PullRequestId == pullRequestId, cancellationToken);

    public async Task<Review?> GetWithIssuesByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
}
