using CodeSheriff.Domain.Common;

namespace CodeSheriff.Domain.Interfaces;

public interface IGitHubService
{
    Task<Result<string>> GetPullRequestDiffAsync(
        long installationId,
        long repoGitHubId,
        int prNumber,
        CancellationToken cancellationToken = default);
}
