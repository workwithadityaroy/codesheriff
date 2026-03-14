using CodeSheriff.Application.Common;
using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;

public sealed record GetPullRequestsByRepositoryQuery(
    Guid RepositoryId,
    int Page = 1,
    int PageSize = 0,       // 0 = return all (preserves existing behaviour)
    string? StatusFilter = null)
    : IRequest<Result<PagedResult<PullRequestSummaryDto>>>;
