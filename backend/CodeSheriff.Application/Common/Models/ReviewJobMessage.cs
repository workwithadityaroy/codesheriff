namespace CodeSheriff.Application.Common.Models;

public sealed record ReviewJobMessage(
    Guid PullRequestId,
    long InstallationId,
    string Owner,
    string RepoName,
    int GitHubPrNumber);
