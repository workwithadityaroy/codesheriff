using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Interfaces;

public interface IGitProvider
{
    GitProvider ProviderType { get; }

    Task<Result<string>> GetPullRequestDiffAsync(
        Entities.Repository repository,
        int prNumber,
        CancellationToken cancellationToken = default);
}
