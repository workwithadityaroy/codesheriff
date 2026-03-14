using CodeSheriff.Application.Settings.Commands.TestAiConnection;
using CodeSheriff.Application.Settings.Commands.UpdateUserSettings;
using CodeSheriff.Application.Settings.Queries.GetUserSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSheriff.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISender _sender;

    public SettingsController(ISender sender) => _sender = sender;

    /// <summary>Returns the current user's settings (defaults if none saved).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserSettingsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Saves the current user's settings.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(UpdateUserSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateUserSettingsCommand(
            request.AiProvider,
            request.AiModel,
            request.AiApiKey,
            request.NotificationEmail,
            request.WeeklyReportEnabled), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    /// <summary>Tests the AI connection with the provided credentials.</summary>
    [HttpPost("test-connection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestConnection(TestConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TestAiConnectionCommand(
            request.AiProvider, request.AiApiKey, request.AiModel), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Connection successful." });
    }
}

public sealed record UpdateUserSettingsRequest(
    string AiProvider,
    string AiModel,
    string AiApiKey,
    string NotificationEmail,
    bool WeeklyReportEnabled);

public sealed record TestConnectionRequest(
    string AiProvider,
    string AiApiKey,
    string AiModel);
