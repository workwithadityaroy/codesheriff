using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal sealed class UserSettingsRepository : BaseRepository<UserSettings>, IUserSettingsRepository
{
    public UserSettingsRepository(CodeSheriffDbContext context) : base(context) { }

    public async Task<UserSettings?> GetByClerkUserIdAsync(
        string clerkUserId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .FirstOrDefaultAsync(s => s.ClerkUserId == clerkUserId, cancellationToken);
}
