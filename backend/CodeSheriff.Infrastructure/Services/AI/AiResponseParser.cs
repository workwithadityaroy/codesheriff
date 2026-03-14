using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Infrastructure.Services.AI;

/// <summary>Shared logic for parsing AI review JSON responses across all providers.</summary>
internal static class AiResponseParser
{
    internal const string SystemPrompt =
        """
        You are a senior software engineer performing a code review. Analyze the pull request diff and respond ONLY with valid JSON — no markdown, no explanation:
        {"techDebtScore":<integer 0-100>,"summary":"<2-3 sentence summary>","issues":[{"severity":"<Info|Warning|Error|Critical>","category":"<Security|Performance|CodeSmell|Maintainability|Design|Other>","filePath":"<path or empty>","lineNumber":<int or null>,"description":"<issue>","suggestion":"<fix>"}]}
        TechDebtScore: 0=clean, 100=critical debt.
        """;

    internal static string BuildUserContent(string repoFullName, int prNumber, string prTitle, string diff)
        => $"Repository: {repoFullName}\nPR #{prNumber}: {prTitle}\n\n{diff}";

    internal static Result<AiReviewResult> Parse(string text)
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

    private static string StripMarkdownCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0) trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3].TrimEnd();
        }
        return trimmed;
    }

    private static T ParseEnum<T>(string? value, T fallback) where T : struct, Enum
        => Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : fallback;

    private sealed class AiResponseDto
    {
        [JsonPropertyName("techDebtScore")] public decimal TechDebtScore { get; init; }
        [JsonPropertyName("summary")] public string? Summary { get; init; }
        [JsonPropertyName("issues")] public List<AiIssueDto>? Issues { get; init; }
    }

    private sealed class AiIssueDto
    {
        [JsonPropertyName("severity")] public string? Severity { get; init; }
        [JsonPropertyName("category")] public string? Category { get; init; }
        [JsonPropertyName("filePath")] public string? FilePath { get; init; }
        [JsonPropertyName("lineNumber")] public int? LineNumber { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("suggestion")] public string? Suggestion { get; init; }
    }
}
