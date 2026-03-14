namespace CodeSheriff.Application.Common.Options;

public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-sonnet-4-6";
}
