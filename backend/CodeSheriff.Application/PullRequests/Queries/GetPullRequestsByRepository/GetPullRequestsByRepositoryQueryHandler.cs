using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;

internal sealed class GetPullRequestsByRepositoryQueryHandler
    : IRequestHandler<GetPullRequestsByRepositoryQuery, Result<IReadOnlyList<PullRequestSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPullRequestsByRepositoryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<PullRequestSummaryDto>>> Handle(
        GetPullRequestsByRepositoryQuery request,
        CancellationToken cancellationToken)
    {
        var repo = await _unitOfWork.Repositories.GetByIdAsync(request.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure<IReadOnlyList<PullRequestSummaryDto>>("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        if (repo.ClerkUserId != userId)
            return Result.Failure<IReadOnlyList<PullRequestSummaryDto>>("Repository not found.");

        var pullRequests = await _unitOfWork.PullRequests.GetByRepositoryIdAsync(request.RepositoryId, cancellationToken);

        var dtos = new List<PullRequestSummaryDto>(pullRequests.Count);
        foreach (var pr in pullRequests)
        {
            var review = await _unitOfWork.Reviews.GetLatestWithIssuesByPullRequestIdAsync(pr.Id, cancellationToken);
            dtos.Add(new PullRequestSummaryDto(
                pr.Id,
                pr.GitHubPrNumber,
                pr.Title,
                pr.HeadBranch,
                pr.BaseBranch,
                pr.AuthorLogin,
                pr.Status.ToString(),
                review?.Id,
                review?.TechDebtScore,
                review?.Status.ToString(),
                pr.CreatedAt,
                pr.UpdatedAt));
        }

        return Result.Success<IReadOnlyList<PullRequestSummaryDto>>(dtos.AsReadOnly());
    }
}
