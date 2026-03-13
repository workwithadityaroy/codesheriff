using CodeSheriff.Domain.Entities;

namespace CodeSheriff.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetLatestByPullRequestIdAsync(Guid pullRequestId, CancellationToken cancellationToken = default);
    Task<Review?> GetWithIssuesByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<Review?> GetLatestWithIssuesByPullRequestIdAsync(Guid pullRequestId, CancellationToken cancellationToken = default);
}
