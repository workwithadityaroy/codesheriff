using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;

public sealed record GetPullRequestsByRepositoryQuery(Guid RepositoryId)
    : IRequest<Result<IReadOnlyList<PullRequestSummaryDto>>>;
