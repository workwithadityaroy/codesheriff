namespace CodeSheriff.Domain.Interfaces;

public interface IRepositoryRepository : IRepository<Entities.Repository>
{
    Task<Entities.Repository?> GetByGitHubIdAsync(long gitHubId, CancellationToken cancellationToken = default);
    Task<Entities.Repository?> GetByFullNameAsync(string fullName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Repository>> GetActiveRepositoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Repository>> GetActiveByClerkUserIdAsync(string clerkUserId, CancellationToken cancellationToken = default);
}
