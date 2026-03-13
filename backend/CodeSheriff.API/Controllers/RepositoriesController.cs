using CodeSheriff.Application.Repositories.Commands.RegisterRepository;
using CodeSheriff.Application.Repositories.Queries.GetRepositories;
using CodeSheriff.Application.Repositories.Queries.GetRepositoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class RepositoriesController : ControllerBase
{
    private readonly ISender _sender;

    public RepositoriesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all monitored repositories for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RepositoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRepositoriesQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Returns a single repository by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RepositoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRepositoryByIdQuery(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Registers a GitHub repository for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        RegisterRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterRepositoryCommand(
            request.GitHubId,
            request.Owner,
            request.Name,
            request.FullName,
            request.InstallationId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }
}

public sealed record RegisterRepositoryRequest(
    long GitHubId,
    string Owner,
    string Name,
    string FullName,
    long InstallationId);
