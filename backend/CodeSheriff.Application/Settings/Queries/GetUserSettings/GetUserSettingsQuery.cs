using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Settings.Queries.GetUserSettings;

public sealed record GetUserSettingsQuery : IRequest<Result<UserSettingsDto>>;

public sealed record UserSettingsDto(
    string AiProvider,
    string AiModel,
    bool HasApiKey,
    string NotificationEmail,
    bool WeeklyReportEnabled);
