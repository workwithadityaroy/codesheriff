using CodeSheriff.Domain.Entities;

namespace CodeSheriff.Domain.Interfaces;

public interface IRepositoryMemberRepository
{
    Task<IReadOnlyList<RepositoryMember>> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid repositoryId, string clerkUserId, CancellationToken cancellationToken = default);
    Task<RepositoryMember?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RepositoryMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetRepositoryIdsByUserAsync(string clerkUserId, CancellationToken cancellationToken = default);
    Task AddAsync(RepositoryMember member, CancellationToken cancellationToken = default);
    void Remove(RepositoryMember member);
}
