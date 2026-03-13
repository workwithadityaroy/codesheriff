using System.Text.Json.Serialization;

namespace CodeSheriff.API.Models;

public sealed record GitHubWebhookPayload(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("number")] int Number,
    [property: JsonPropertyName("pull_request")] GitHubPullRequestPayload PullRequest,
    [property: JsonPropertyName("repository")] GitHubRepositoryPayload Repository
);

public sealed record GitHubPullRequestPayload(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("head")] GitHubRefPayload Head,
    [property: JsonPropertyName("base")] GitHubRefPayload Base,
    [property: JsonPropertyName("user")] GitHubUserPayload User
);

public sealed record GitHubRefPayload(
    [property: JsonPropertyName("ref")] string Ref
);

public sealed record GitHubUserPayload(
    [property: JsonPropertyName("login")] string Login
);

public sealed record GitHubRepositoryPayload(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("full_name")] string FullName
);
