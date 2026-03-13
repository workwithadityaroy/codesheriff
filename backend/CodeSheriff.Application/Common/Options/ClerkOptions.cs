namespace CodeSheriff.Application.Common.Options;

public sealed class ClerkOptions
{
    public const string SectionName = "Clerk";
    public string SecretKey { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
}
