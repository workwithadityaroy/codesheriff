namespace CodeSheriff.Application.Common.Options;

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";
    public string WebhookSecret { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
}
