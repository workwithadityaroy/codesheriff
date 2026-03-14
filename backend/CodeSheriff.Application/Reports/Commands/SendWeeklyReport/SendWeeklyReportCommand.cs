using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Reports.Commands.SendWeeklyReport;

public sealed record SendWeeklyReportCommand(
    string ClerkUserId,
    string UserEmail,
    string DisplayName) : IRequest<Result>;
