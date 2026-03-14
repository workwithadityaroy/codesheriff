using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Services.AI.Providers;

internal sealed class OpenAiProvider : IAiProvider
{
    public string ProviderKey => "openai";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAiProvider> _logger;

    public OpenAiProvider(IHttpClientFactory httpClientFactory, ILogger<OpenAiProvider> logger)
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
        var resolvedModel = string.IsNullOrEmpty(model) ? "gpt-4o-mini" : model;
        var userContent = AiResponseParser.BuildUserContent(repoFullName, prNumber, prTitle, diff);

        var requestBody = new
        {
            model = resolvedModel,
            max_tokens = 4096,
            messages = new[]
            {
                new { role = "system", content = AiResponseParser.SystemPrompt },
                new { role = "user", content = userContent }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("openai");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error ({Status}): {Body}", response.StatusCode, rawBody);
                return Result.Failure<AiReviewResult>($"OpenAI API error ({response.StatusCode})");
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
            _logger.LogError(ex, "Error calling OpenAI API");
            return Result.Failure<AiReviewResult>($"OpenAI API failed: {ex.Message}");
        }
    }
}
