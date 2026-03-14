using CodeSheriff.Application.Members;
using CodeSheriff.Application.Members.Commands.AcceptInvite;
using CodeSheriff.Application.Members.Commands.InviteMember;
using CodeSheriff.Application.Members.Commands.RemoveMember;
using CodeSheriff.Application.Members.Queries.GetRepositoryMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/repositories/{repoId:guid}/members")]
public sealed class MembersController : ControllerBase
{
    private readonly ISender _sender;

    public MembersController(ISender sender) => _sender = sender;

    /// <summary>Returns all members for a repository (owner + invited).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(Guid repoId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRepositoryMembersQuery(repoId), cancellationToken);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>Invites a new member to the repository (owner only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InviteMemberResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Invite(
        Guid repoId, InviteMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new InviteMemberCommand(repoId, request.Email, request.Role), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetMembers), new { repoId }, result.Value);
    }

    /// <summary>Removes a member from the repository (owner only).</summary>
    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(
        Guid repoId, Guid memberId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveMemberCommand(repoId, memberId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/v1/invites")]
public sealed class InvitesController : ControllerBase
{
    private readonly ISender _sender;

    public InvitesController(ISender sender) => _sender = sender;

    /// <summary>Accepts an invite token and links the current user to the repository.</summary>
    [HttpPost("{token}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptInviteCommand(token), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return Ok(new { repositoryId = result.Value });
    }
}

public sealed record InviteMemberRequest(string Email, string Role);
