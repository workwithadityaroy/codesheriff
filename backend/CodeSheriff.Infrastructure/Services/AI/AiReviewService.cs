using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSheriff.Infrastructure.Services.AI;

internal sealed class AiReviewService : IAiReviewService
{
    private const string SystemPrompt =
        """
        You are a senior software engineer performing a code review. Analyze the pull request diff and respond ONLY with valid JSON — no markdown, no explanation:
        {"techDebtScore":<integer 0-100>,"summary":"<2-3 sentence summary>","issues":[{"severity":"<Info|Warning|Error|Critical>","category":"<Security|Performance|CodeSmell|Maintainability|Design|Other>","filePath":"<path or empty>","lineNumber":<int or null>,"description":"<issue>","suggestion":"<fix>"}]}
        TechDebtScore: 0=clean, 100=critical debt.
        """;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AnthropicOptions _options;
    private readonly ILogger<AiReviewService> _logger;

    public AiReviewService(
        IHttpClientFactory httpClientFactory,
        IOptions<AnthropicOptions> options,
        ILogger<AiReviewService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<AiReviewResult>> ReviewPullRequestAsync(
        string diff,
        string repoFullName,
        int prNumber,
        string prTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Anthropic API key is not configured. Returning stub result.");
            return Result.Success(new AiReviewResult(
                TechDebtScore: 0,
                Summary: "AI review not configured. Set Anthropic:ApiKey.",
                RawResponse: string.Empty,
                Issues: []));
        }

        try
        {
            var userContent = $"Repository: {repoFullName}\nPR #{prNumber}: {prTitle}\n\n{diff}";

            var requestBody = new
            {
                model = _options.Model,
                max_tokens = 4096,
                system = SystemPrompt,
                messages = new[] { new { role = "user", content = userContent } }
            };

            var client = _httpClientFactory.CreateClient("anthropic");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
            request.Headers.Add("x-api-key", _options.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error ({Status}): {Body}", response.StatusCode, rawBody);
                return Result.Failure<AiReviewResult>($"Anthropic API error ({response.StatusCode})");
            }

            using var doc = JsonDocument.Parse(rawBody);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return ParseAiResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Anthropic API");
            return Result.Failure<AiReviewResult>($"AI review failed: {ex.Message}");
        }
    }

    private static string StripMarkdownCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
                trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..^3].TrimEnd();
        }
        return trimmed;
    }

    private static Result<AiReviewResult> ParseAiResponse(string text)
    {
        try
        {
            text = StripMarkdownCodeFences(text);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<AiResponseDto>(text, options);
            if (parsed is null)
                return Result.Failure<AiReviewResult>("AI returned empty response.");

            var score = Math.Clamp(parsed.TechDebtScore, 0m, 100m);

            var issues = (parsed.Issues ?? []).Select(i => new AiReviewIssue(
                ParseEnum<IssueSeverity>(i.Severity, IssueSeverity.Info),
                ParseEnum<IssueCategory>(i.Category, IssueCategory.Other),
                i.FilePath ?? string.Empty,
                i.LineNumber,
                i.Description ?? string.Empty,
                i.Suggestion ?? string.Empty)).ToList();

            return Result.Success(new AiReviewResult(score, parsed.Summary ?? string.Empty, text, issues));
        }
        catch (Exception ex)
        {
            return Result.Failure<AiReviewResult>($"Failed to parse AI response: {ex.Message}");
        }
    }

    private static T ParseEnum<T>(string? value, T fallback) where T : struct, Enum
        => Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : fallback;

    private sealed class AiResponseDto
    {
        [JsonPropertyName("techDebtScore")]
        public decimal TechDebtScore { get; init; }

        [JsonPropertyName("summary")]
        public string? Summary { get; init; }

        [JsonPropertyName("issues")]
        public List<AiIssueDto>? Issues { get; init; }
    }

    private sealed class AiIssueDto
    {
        [JsonPropertyName("severity")]
        public string? Severity { get; init; }

        [JsonPropertyName("category")]
        public string? Category { get; init; }

        [JsonPropertyName("filePath")]
        public string? FilePath { get; init; }

        [JsonPropertyName("lineNumber")]
        public int? LineNumber { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("suggestion")]
        public string? Suggestion { get; init; }
    }
}
