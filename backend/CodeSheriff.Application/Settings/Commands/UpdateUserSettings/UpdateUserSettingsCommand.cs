using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Settings.Commands.UpdateUserSettings;

public sealed record UpdateUserSettingsCommand(
    string AiProvider,
    string AiModel,
    string AiApiKey,
    string NotificationEmail,
    bool WeeklyReportEnabled) : IRequest<Result>;
