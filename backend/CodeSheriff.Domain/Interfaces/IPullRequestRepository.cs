using CodeSheriff.Domain.Entities;

namespace CodeSheriff.Domain.Interfaces;

public interface IPullRequestRepository : IRepository<PullRequest>
{
    Task<PullRequest?> GetByGitHubPrNumberAsync(Guid repositoryId, int prNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PullRequest>> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<PullRequest> Items, int TotalCount)> GetPagedByRepositoryIdAsync(
        Guid repositoryId,
        int page,
        int pageSize,
        string? statusFilter,
        CancellationToken cancellationToken = default);
}
