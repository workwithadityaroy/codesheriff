using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSheriff.Infrastructure.Services.GitHub;

internal sealed class GitHubService : IGitHubService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GitHubOptions _options;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(
        IHttpClientFactory httpClientFactory,
        IOptions<GitHubOptions> options,
        ILogger<GitHubService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<string>> GetPullRequestDiffAsync(
        long installationId,
        string owner,
        string repoName,
        int prNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jwt = GenerateAppJwt();
            var tokenResult = await GetInstallationTokenAsync(jwt, installationId, cancellationToken);
            if (!tokenResult.IsSuccess)
                return Result.Failure<string>(tokenResult.Error);

            var diff = await FetchPullRequestDiffAsync(tokenResult.Value!, owner, repoName, prNumber, cancellationToken);
            return diff;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PR diff for {Owner}/{Repo}#{PrNumber}", owner, repoName, prNumber);
            return Result.Failure<string>($"Failed to fetch PR diff: {ex.Message}");
        }
    }

    private string GenerateAppJwt()
    {
        var now = DateTimeOffset.UtcNow;
        var iat = now.AddSeconds(-60).ToUnixTimeSeconds();
        var exp = now.AddMinutes(9).ToUnixTimeSeconds();

        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { iat, exp, iss = _options.AppId }));

        var signingInput = $"{header}.{payload}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(_options.PrivateKey.AsSpan());

        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{signingInput}.{Base64UrlEncode(signature)}";
    }

    private async Task<Result<string>> GetInstallationTokenAsync(
        string jwt, long installationId, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("github");
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/app/installations/{installationId}/access_tokens");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<string>($"GitHub token exchange failed ({response.StatusCode}): {body}");
        }

        var json = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(json, cancellationToken: cancellationToken);
        var token = doc.RootElement.GetProperty("token").GetString();
        return Result.Success(token!);
    }

    private async Task<Result<string>> FetchPullRequestDiffAsync(
        string token, string owner, string repoName, int prNumber, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("github");
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/repos/{owner}/{repoName}/pulls/{prNumber}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3.diff"));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<string>($"GitHub diff fetch failed ({response.StatusCode}): {body}");
        }

        var diff = await response.Content.ReadAsStringAsync(cancellationToken);
        if (diff.Length > 100_000)
            diff = diff[..100_000];

        return Result.Success(diff);
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
