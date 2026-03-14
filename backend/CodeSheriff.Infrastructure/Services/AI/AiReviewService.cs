using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using CodeSheriff.Infrastructure.Services.AI.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSheriff.Infrastructure.Services.AI;

internal sealed class AiReviewService : IAiReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AnthropicOptions _defaultOptions;
    private readonly IReadOnlyDictionary<string, IAiProvider> _providers;
    private readonly ILogger<AiReviewService> _logger;

    public AiReviewService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<AnthropicOptions> defaultOptions,
        ClaudeAiProvider claudeProvider,
        OpenAiProvider openAiProvider,
        AzureOpenAiProvider azureProvider,
        ILogger<AiReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _defaultOptions = defaultOptions.Value;
        _logger = logger;
        _providers = new Dictionary<string, IAiProvider>(StringComparer.OrdinalIgnoreCase)
        {
            [claudeProvider.ProviderKey] = claudeProvider,
            [openAiProvider.ProviderKey] = openAiProvider,
            [azureProvider.ProviderKey] = azureProvider,
        };
    }

    public async Task<Result<AiReviewResult>> ReviewPullRequestAsync(
        string diff,
        string repoFullName,
        int prNumber,
        string prTitle,
        CancellationToken cancellationToken = default)
    {
        // Resolve provider key, API key and model from user settings (fall back to app config)
        string providerKey = "claude";
        string apiKey = _defaultOptions.ApiKey;
        string model = _defaultOptions.Model;

        var clerkUserId = _currentUserService.GetClerkUserId();
        if (!string.IsNullOrEmpty(clerkUserId))
        {
            var settings = await _unitOfWork.UserSettings.GetByClerkUserIdAsync(clerkUserId, cancellationToken);
            if (settings is not null)
            {
                providerKey = settings.AiProvider;
                if (!string.IsNullOrEmpty(settings.AiApiKey)) apiKey = settings.AiApiKey;
                if (!string.IsNullOrEmpty(settings.AiModel)) model = settings.AiModel;
            }
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("No AI API key configured. Returning stub result.");
            return Result.Success(new AiReviewResult(
                TechDebtScore: 0,
                Summary: "AI review not configured. Add your API key in Settings.",
                RawResponse: string.Empty,
                Issues: []));
        }

        if (!_providers.TryGetValue(providerKey, out var provider))
        {
            _logger.LogWarning("Unknown AI provider '{Provider}', falling back to Claude.", providerKey);
            provider = _providers["claude"];
        }

        _logger.LogInformation("Using AI provider '{Provider}' for {RepoFullName} PR #{PrNumber}", providerKey, repoFullName, prNumber);

        return await provider.ReviewAsync(diff, repoFullName, prNumber, prTitle, apiKey, model, cancellationToken);
    }

    public async Task<Result> TestConnectionAsync(
        string providerKey,
        string apiKey,
        string model,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(providerKey, out var provider))
            return Result.Failure($"Unknown AI provider '{providerKey}'.");

        const string testDiff = "diff --git a/hello.cs b/hello.cs\n+++ b/hello.cs\n+Console.WriteLine(\"hello\");";
        var result = await provider.ReviewAsync(testDiff, "test/connection", 0, "Connection test", apiKey, model, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}
