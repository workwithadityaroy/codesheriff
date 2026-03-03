using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;

namespace CodeSheriff.Infrastructure.Services.GitHub;

internal sealed class GitHubService : IGitHubService
{
    public Task<Result<string>> GetPullRequestDiffAsync(
        long installationId,
        long repoGitHubId,
        int prNumber,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<string>("GitHub service not yet implemented."));
}
