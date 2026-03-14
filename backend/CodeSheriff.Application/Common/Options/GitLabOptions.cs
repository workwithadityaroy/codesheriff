namespace CodeSheriff.Application.Common.Options;

public sealed class GitLabOptions
{
    public const string SectionName = "GitLab";
    public string WebhookSecret { get; set; } = string.Empty;
}
