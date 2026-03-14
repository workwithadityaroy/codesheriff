using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;

namespace CodeSheriff.Infrastructure.Services.Git;

internal sealed class GitProviderFactory : IGitProviderFactory
{
    private readonly IReadOnlyDictionary<GitProvider, IGitProvider> _providers;

    public GitProviderFactory(IEnumerable<IGitProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderType);
    }

    public IGitProvider GetProvider(GitProvider providerType)
    {
        if (_providers.TryGetValue(providerType, out var provider))
            return provider;

        // Fall back to GitHub if an unknown provider is requested
        return _providers[GitProvider.GitHub];
    }
}
