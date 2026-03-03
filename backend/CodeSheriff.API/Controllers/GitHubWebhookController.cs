using System.Text.Json;
using CodeSheriff.API.Models;
using CodeSheriff.API.Services;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Route("api/v1/github")]
public sealed class GitHubWebhookController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GitHubOptions _gitHubOptions;

    public GitHubWebhookController(IUnitOfWork unitOfWork, IOptions<GitHubOptions> gitHubOptions)
    {
        _unitOfWork = unitOfWork;
        _gitHubOptions = gitHubOptions.Value;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms, cancellationToken);
        var body = ms.ToArray();

        var signatureHeader = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (!WebhookSignatureValidator.IsValid(body, _gitHubOptions.WebhookSecret, signatureHeader))
            return Unauthorized();

        var eventType = Request.Headers["X-GitHub-Event"].FirstOrDefault();

        if (eventType == "ping")
            return Ok();

        if (eventType != "pull_request")
            return Ok();

        var payload = JsonSerializer.Deserialize<GitHubWebhookPayload>(body);
        if (payload is null)
            return Ok();

        var action = payload.Action;
        if (action is not ("opened" or "synchronize" or "reopened"))
            return Ok();

        var repo = await _unitOfWork.Repositories.GetByGitHubIdAsync(
            payload.Repository.Id, cancellationToken);

        if (repo is null)
            return Ok();

        var existingPr = await _unitOfWork.PullRequests.GetByGitHubPrNumberAsync(
            repo.Id, payload.Number, cancellationToken);

        if (existingPr is null)
        {
            var pr = PullRequest.Create(
                repo.Id,
                payload.Number,
                payload.PullRequest.Title,
                payload.PullRequest.Head.Ref,
                payload.PullRequest.Base.Ref,
                payload.PullRequest.User.Login);

            await _unitOfWork.PullRequests.AddAsync(pr, cancellationToken);
        }
        else
        {
            existingPr.MarkAsReviewing();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}
