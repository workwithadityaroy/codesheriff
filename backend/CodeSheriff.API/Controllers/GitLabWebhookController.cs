using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Route("api/v1/gitlab")]
public sealed class GitLabWebhookController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GitLabOptions _gitLabOptions;
    private readonly IReviewQueueService _reviewQueueService;

    public GitLabWebhookController(
        IUnitOfWork unitOfWork,
        IOptions<GitLabOptions> gitLabOptions,
        IReviewQueueService reviewQueueService)
    {
        _unitOfWork = unitOfWork;
        _gitLabOptions = gitLabOptions.Value;
        _reviewQueueService = reviewQueueService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        // Validate GitLab webhook token
        var tokenHeader = Request.Headers["X-Gitlab-Token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(_gitLabOptions.WebhookSecret) &&
            tokenHeader != _gitLabOptions.WebhookSecret)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid GitLab webhook token.");
        }

        var eventType = Request.Headers["X-Gitlab-Event"].FirstOrDefault();
        if (eventType is not "Merge Request Hook")
            return Ok();

        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms, cancellationToken);
        var payload = JsonSerializer.Deserialize<GitLabMrPayload>(ms.ToArray(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        if (payload is null) return Ok();

        var attrs = payload.ObjectAttributes;
        if (attrs is null) return Ok();

        // Only process open/update/reopen actions
        if (attrs.Action is not ("open" or "update" or "reopen")) return Ok();
        if (attrs.State is not "opened") return Ok();

        var fullName = payload.Project?.PathWithNamespace;
        if (string.IsNullOrEmpty(fullName)) return Ok();

        var repo = await _unitOfWork.Repositories.GetByFullNameAsync(fullName, cancellationToken);
        if (repo is null) return Ok();

        Guid pullRequestId;

        var existingPr = await _unitOfWork.PullRequests.GetByGitHubPrNumberAsync(
            repo.Id, attrs.Iid, cancellationToken);

        if (existingPr is null)
        {
            var pr = PullRequest.Create(
                repo.Id,
                attrs.Iid,
                attrs.Title ?? string.Empty,
                attrs.SourceBranch ?? string.Empty,
                attrs.TargetBranch ?? string.Empty,
                payload.User?.Username ?? string.Empty);

            await _unitOfWork.PullRequests.AddAsync(pr, cancellationToken);
            pullRequestId = pr.Id;
        }
        else
        {
            existingPr.MarkAsReviewing();
            pullRequestId = existingPr.Id;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var hasActive = await _unitOfWork.Reviews.HasActiveReviewAsync(pullRequestId, cancellationToken);
        if (hasActive) return Ok();

        await _reviewQueueService.EnqueueAsync(
            new ReviewJobMessage(
                pullRequestId,
                repo.InstallationId,
                repo.Owner,
                repo.Name,
                attrs.Iid),
            cancellationToken);

        return Ok();
    }
}

// GitLab webhook payload models
file sealed class GitLabMrPayload
{
    public string? ObjectKind { get; set; }
    public GitLabProject? Project { get; set; }
    public GitLabMrUser? User { get; set; }
    public GitLabMrAttributes? ObjectAttributes { get; set; }
}

file sealed class GitLabProject
{
    public long Id { get; set; }
    [JsonPropertyName("path_with_namespace")]
    public string? PathWithNamespace { get; set; }
}

file sealed class GitLabMrUser
{
    public string? Username { get; set; }
}

file sealed class GitLabMrAttributes
{
    public int Iid { get; set; }
    public string? Title { get; set; }
    [JsonPropertyName("source_branch")]
    public string? SourceBranch { get; set; }
    [JsonPropertyName("target_branch")]
    public string? TargetBranch { get; set; }
    public string? Action { get; set; }
    public string? State { get; set; }
}
