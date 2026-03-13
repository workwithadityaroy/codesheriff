using CodeSheriff.Domain.Common;

namespace CodeSheriff.Domain.Interfaces;

public interface IGitHubService
{
    Task<Result<string>> GetPullRequestDiffAsync(
        long installationId,
        string owner,
        string repoName,
        int prNumber,
        CancellationToken cancellationToken = default);
}
