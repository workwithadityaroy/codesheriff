using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;

namespace CodeSheriff.Infrastructure.Services.GitHub;

internal sealed class GitHubProvider : IGitProvider
{
    private readonly IGitHubService _gitHubService;

    public GitHubProvider(IGitHubService gitHubService) => _gitHubService = gitHubService;

    public GitProvider ProviderType => GitProvider.GitHub;

    public Task<Result<string>> GetPullRequestDiffAsync(
        Domain.Entities.Repository repository,
        int prNumber,
        CancellationToken cancellationToken = default)
        => _gitHubService.GetPullRequestDiffAsync(
            repository.InstallationId, repository.Owner, repository.Name, prNumber, cancellationToken);
}
