using System.Text;
using System.Text.Json;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Services.AI.Providers;

internal sealed class ClaudeAiProvider : IAiProvider
{
    public string ProviderKey => "claude";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClaudeAiProvider> _logger;

    public ClaudeAiProvider(IHttpClientFactory httpClientFactory, ILogger<ClaudeAiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<AiReviewResult>> ReviewAsync(
        string diff,
        string repoFullName,
        int prNumber,
        string prTitle,
        string apiKey,
        string model,
        CancellationToken cancellationToken = default)
    {
        var resolvedModel = string.IsNullOrEmpty(model) ? "claude-haiku-4-5-20251001" : model;
        var userContent = AiResponseParser.BuildUserContent(repoFullName, prNumber, prTitle, diff);

        var requestBody = new
        {
            model = resolvedModel,
            max_tokens = 4096,
            system = AiResponseParser.SystemPrompt,
            messages = new[] { new { role = "user", content = userContent } }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("anthropic");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
            request.Headers.Add("x-api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error ({Status}): {Body}", response.StatusCode, rawBody);
                return Result.Failure<AiReviewResult>($"Claude API error ({response.StatusCode})");
            }

            using var doc = JsonDocument.Parse(rawBody);
            var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;
            return AiResponseParser.Parse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return Result.Failure<AiReviewResult>($"Claude API failed: {ex.Message}");
        }
    }
}
