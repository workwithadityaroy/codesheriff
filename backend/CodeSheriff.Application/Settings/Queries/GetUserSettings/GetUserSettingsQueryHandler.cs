using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Settings.Queries.GetUserSettings;

internal sealed class GetUserSettingsQueryHandler
    : IRequestHandler<GetUserSettingsQuery, Result<UserSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserSettingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserSettingsDto>> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetClerkUserId();
        var settings = await _unitOfWork.UserSettings.GetByClerkUserIdAsync(userId, cancellationToken);

        // Return defaults if no settings saved yet
        var dto = settings is null
            ? new UserSettingsDto("claude", string.Empty, false, string.Empty, true)
            : new UserSettingsDto(
                settings.AiProvider,
                settings.AiModel,
                !string.IsNullOrEmpty(settings.AiApiKey),
                settings.NotificationEmail,
                settings.WeeklyReportEnabled);

        return Result.Success(dto);
    }
}
