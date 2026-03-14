using CodeSheriff.Domain.Entities;

namespace CodeSheriff.Domain.Interfaces;

public interface IUserSettingsRepository : IRepository<UserSettings>
{
    Task<UserSettings?> GetByClerkUserIdAsync(string clerkUserId, CancellationToken cancellationToken = default);
}
