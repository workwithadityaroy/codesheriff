using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Interfaces;

public interface IGitProviderFactory
{
    IGitProvider GetProvider(GitProvider providerType);
}
