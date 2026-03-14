using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class RepositoryMemberRepository : IRepositoryMemberRepository
{
    private readonly CodeSheriffDbContext _context;

    public RepositoryMemberRepository(CodeSheriffDbContext context) => _context = context;

    public async Task<IReadOnlyList<RepositoryMember>> GetByRepositoryIdAsync(
        Guid repositoryId, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers
            .AsNoTracking()
            .Where(m => m.RepositoryId == repositoryId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsMemberAsync(
        Guid repositoryId, string clerkUserId, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers
            .AnyAsync(m => m.RepositoryId == repositoryId
                        && m.ClerkUserId == clerkUserId
                        && m.AcceptedAt != null, cancellationToken);

    public async Task<RepositoryMember?> GetByTokenAsync(
        string token, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers
            .FirstOrDefaultAsync(m => m.InviteToken == token, cancellationToken);

    public async Task<RepositoryMember?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetRepositoryIdsByUserAsync(
        string clerkUserId, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers
            .AsNoTracking()
            .Where(m => m.ClerkUserId == clerkUserId && m.AcceptedAt != null)
            .Select(m => m.RepositoryId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(RepositoryMember member, CancellationToken cancellationToken = default)
        => await _context.RepositoryMembers.AddAsync(member, cancellationToken);

    public void Remove(RepositoryMember member)
        => _context.RepositoryMembers.Remove(member);
}
