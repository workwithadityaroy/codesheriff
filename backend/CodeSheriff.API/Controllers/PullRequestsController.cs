using CodeSheriff.Application.PullRequests.Commands.ReanalyzePullRequest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pull-requests")]
public sealed class PullRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public PullRequestsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Triggers a fresh AI review for an existing pull request.</summary>
    [HttpPost("{id:guid}/reanalyze")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reanalyze(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReanalyzePullRequestCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Accepted();
    }
}
