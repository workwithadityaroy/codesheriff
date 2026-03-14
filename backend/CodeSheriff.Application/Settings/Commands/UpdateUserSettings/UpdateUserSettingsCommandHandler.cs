using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Settings.Commands.UpdateUserSettings;

internal sealed class UpdateUserSettingsCommandHandler
    : IRequestHandler<UpdateUserSettingsCommand, Result>
{
    private static readonly HashSet<string> ValidProviders = new(StringComparer.OrdinalIgnoreCase)
        { "claude", "openai", "azure-openai" };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserSettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        if (!ValidProviders.Contains(request.AiProvider))
            return Result.Failure($"Unknown AI provider '{request.AiProvider}'. Valid: {string.Join(", ", ValidProviders)}.");

        var userId = _currentUserService.GetClerkUserId();
        var settings = await _unitOfWork.UserSettings.GetByClerkUserIdAsync(userId, cancellationToken);

        if (settings is null)
        {
            settings = UserSettings.Create(userId);
            settings.Update(request.AiProvider, request.AiModel, request.AiApiKey,
                request.NotificationEmail, request.WeeklyReportEnabled);
            await _unitOfWork.UserSettings.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.Update(request.AiProvider, request.AiModel, request.AiApiKey,
                request.NotificationEmail, request.WeeklyReportEnabled);
            _unitOfWork.UserSettings.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
