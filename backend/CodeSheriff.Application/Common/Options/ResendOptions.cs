namespace CodeSheriff.Application.Common.Options;

public sealed class ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "reports@codesheriff.dev";
    public string FromName { get; init; } = "CodeSheriff";
}
