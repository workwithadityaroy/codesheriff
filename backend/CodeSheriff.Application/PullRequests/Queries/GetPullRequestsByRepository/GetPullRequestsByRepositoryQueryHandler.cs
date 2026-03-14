using CodeSheriff.Application.Common;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;

internal sealed class GetPullRequestsByRepositoryQueryHandler
    : IRequestHandler<GetPullRequestsByRepositoryQuery, Result<PagedResult<PullRequestSummaryDto>>>
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

    public async Task<Result<PagedResult<PullRequestSummaryDto>>> Handle(
        GetPullRequestsByRepositoryQuery request,
        CancellationToken cancellationToken)
    {
        var repo = await _unitOfWork.Repositories.GetByIdAsync(request.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure<PagedResult<PullRequestSummaryDto>>("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        var isOwner = repo.ClerkUserId == userId;
        var isMember = !isOwner && await _unitOfWork.Members.IsMemberAsync(repo.Id, userId, cancellationToken);
        if (!isOwner && !isMember)
            return Result.Failure<PagedResult<PullRequestSummaryDto>>("Repository not found.");

        IReadOnlyList<PullRequest> pullRequests;
        int totalCount;

        if (request.PageSize > 0)
        {
            (pullRequests, totalCount) = await _unitOfWork.PullRequests.GetPagedByRepositoryIdAsync(
                request.RepositoryId,
                request.Page,
                request.PageSize,
                request.StatusFilter,
                cancellationToken);
        }
        else
        {
            pullRequests = await _unitOfWork.PullRequests.GetByRepositoryIdAsync(
                request.RepositoryId, cancellationToken);
            totalCount = pullRequests.Count;
        }

        var dtos = new List<PullRequestSummaryDto>(pullRequests.Count);
        foreach (var pr in pullRequests)
        {
            var review = await _unitOfWork.Reviews.GetLatestWithIssuesByPullRequestIdAsync(
                pr.Id, cancellationToken);
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

        var page = request.PageSize > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : totalCount;

        return Result.Success(new PagedResult<PullRequestSummaryDto>(
            dtos.AsReadOnly(), totalCount, page, pageSize));
    }
}
