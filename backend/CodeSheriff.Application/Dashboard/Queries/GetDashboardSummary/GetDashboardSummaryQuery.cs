using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery() : IRequest<Result<DashboardSummaryDto>>;
