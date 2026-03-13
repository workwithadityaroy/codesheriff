using CodeSheriff.Domain.Common;

namespace CodeSheriff.Domain.Entities;

public class Repository : Entity
{
    public long GitHubId { get; private set; }
    public string Owner { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public long InstallationId { get; private set; }
    public string ClerkUserId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<PullRequest> PullRequests { get; private set; } = new List<PullRequest>();
    public ICollection<WeeklyReport> WeeklyReports { get; private set; } = new List<WeeklyReport>();

    private Repository() { }

    public static Repository Create(
        long gitHubId,
        string owner,
        string name,
        string fullName,
        long installationId,
        string clerkUserId = "")
    {
        return new Repository
        {
            GitHubId = gitHubId,
            Owner = owner,
            Name = name,
            FullName = fullName,
            InstallationId = installationId,
            ClerkUserId = clerkUserId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateInstallation(long newInstallationId)
    {
        InstallationId = newInstallationId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
