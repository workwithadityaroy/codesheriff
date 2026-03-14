using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;

namespace CodeSheriff.Application.Common.Interfaces;

/// <summary>Abstraction over AI review backends (Claude, OpenAI, Azure OpenAI, etc.).</summary>
public interface IAiProvider
{
    /// <summary>Short key identifying the provider, e.g. "claude", "openai", "azure-openai".</summary>
    string ProviderKey { get; }

    Task<Result<AiReviewResult>> ReviewAsync(
        string diff,
        string repoFullName,
        int prNumber,
        string prTitle,
        string apiKey,
        string model,
        CancellationToken cancellationToken = default);
}
