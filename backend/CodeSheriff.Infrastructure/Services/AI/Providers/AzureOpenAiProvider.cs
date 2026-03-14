using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Services.AI.Providers;

/// <summary>
/// Azure OpenAI provider. The "model" field should be the deployment name.
/// The API key format should be "endpoint|key", e.g. "https://my-resource.openai.azure.com/|abc123".
/// </summary>
internal sealed class AzureOpenAiProvider : IAiProvider
{
    public string ProviderKey => "azure-openai";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AzureOpenAiProvider> _logger;

    public AzureOpenAiProvider(IHttpClientFactory httpClientFactory, ILogger<AzureOpenAiProvider> logger)
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
        // apiKey format: "https://resource.openai.azure.com/|actual-api-key"
        var parts = apiKey.Split('|', 2);
        if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
            return Result.Failure<AiReviewResult>(
                "Azure OpenAI API key must be in format: endpoint|key (e.g. https://resource.openai.azure.com/|abc123)");

        var endpoint = parts[0].TrimEnd('/');
        var key = parts[1];
        var deploymentName = string.IsNullOrEmpty(model) ? "gpt-4o-mini" : model;
        var url = $"{endpoint}/openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-01";

        var userContent = AiResponseParser.BuildUserContent(repoFullName, prNumber, prTitle, diff);
        var requestBody = new
        {
            max_tokens = 4096,
            messages = new[]
            {
                new { role = "system", content = AiResponseParser.SystemPrompt },
                new { role = "user", content = userContent }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", key);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Azure OpenAI error ({Status}): {Body}", response.StatusCode, rawBody);
                return Result.Failure<AiReviewResult>($"Azure OpenAI error ({response.StatusCode})");
            }

            using var doc = JsonDocument.Parse(rawBody);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return AiResponseParser.Parse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            return Result.Failure<AiReviewResult>($"Azure OpenAI failed: {ex.Message}");
        }
    }
}
