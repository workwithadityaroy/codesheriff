using System.Net.Http.Headers;
using System.Text.Json;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Services.GitLab;

internal sealed class GitLabProvider : IGitProvider
{
    private const int MaxDiffChars = 100_000;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitLabProvider> _logger;

    public GitLabProvider(IHttpClientFactory httpClientFactory, ILogger<GitLabProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public GitProvider ProviderType => GitProvider.GitLab;

    public async Task<Result<string>> GetPullRequestDiffAsync(
        Domain.Entities.Repository repository,
        int prNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repository.AccessToken))
            return Result.Failure<string>("GitLab access token is not configured for this repository.");

        var projectPath = Uri.EscapeDataString(repository.FullName);
        var client = _httpClientFactory.CreateClient("gitlab");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", repository.AccessToken);

        _logger.LogInformation(
            "Fetching GitLab MR !{MrNumber} diff for {FullName}", prNumber, repository.FullName);

        var url = $"api/v4/projects/{projectPath}/merge_requests/{prNumber}/diffs?per_page=50";
        var response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "GitLab API returned {StatusCode} for {FullName} MR !{MrNumber}",
                (int)response.StatusCode, repository.FullName, prNumber);
            return Result.Failure<string>($"GitLab API error: HTTP {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var diffs = JsonSerializer.Deserialize<GitLabDiffEntry[]>(content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        if (diffs is null || diffs.Length == 0)
            return Result.Failure<string>("No diff returned from GitLab.");

        var combined = string.Join("\n", diffs
            .Where(d => !string.IsNullOrEmpty(d.Diff))
            .Select(d => $"--- a/{d.OldPath}\n+++ b/{d.NewPath}\n{d.Diff}"));

        if (combined.Length > MaxDiffChars)
            combined = combined[..MaxDiffChars] + "\n[diff truncated]";

        return Result.Success(combined);
    }

    private sealed record GitLabDiffEntry(string OldPath, string NewPath, string Diff);
}
