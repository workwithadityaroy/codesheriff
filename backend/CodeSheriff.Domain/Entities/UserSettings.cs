using CodeSheriff.Domain.Common;

namespace CodeSheriff.Domain.Entities;

public class UserSettings : Entity
{
    public string ClerkUserId { get; private set; } = string.Empty;
    public string AiProvider { get; private set; } = "claude";
    public string AiModel { get; private set; } = string.Empty;
    public string AiApiKey { get; private set; } = string.Empty;
    public string NotificationEmail { get; private set; } = string.Empty;
    public bool WeeklyReportEnabled { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserSettings() { }

    public static UserSettings Create(string clerkUserId) => new()
    {
        ClerkUserId = clerkUserId,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    public void Update(
        string aiProvider,
        string aiModel,
        string aiApiKey,
        string notificationEmail,
        bool weeklyReportEnabled)
    {
        AiProvider = aiProvider;
        AiModel = aiModel;
        AiApiKey = aiApiKey;
        NotificationEmail = notificationEmail;
        WeeklyReportEnabled = weeklyReportEnabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
