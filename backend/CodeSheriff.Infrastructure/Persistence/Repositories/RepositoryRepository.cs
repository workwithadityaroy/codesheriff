using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using DomainRepository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class RepositoryRepository : BaseRepository<DomainRepository>, IRepositoryRepository
{
    public RepositoryRepository(CodeSheriffDbContext context) : base(context) { }

    public async Task<DomainRepository?> GetByGitHubIdAsync(long gitHubId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.GitHubId == gitHubId, cancellationToken);

    public async Task<DomainRepository?> GetByFullNameAsync(string fullName, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FullName == fullName, cancellationToken);

    public async Task<IReadOnlyList<DomainRepository>> GetActiveRepositoriesAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.FullName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DomainRepository>> GetActiveByClerkUserIdAsync(
        string clerkUserId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(r => r.IsActive && r.ClerkUserId == clerkUserId)
            .OrderBy(r => r.FullName)
            .ToListAsync(cancellationToken);
}
