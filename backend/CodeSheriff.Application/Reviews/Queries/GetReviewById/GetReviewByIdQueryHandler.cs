using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Reviews.Queries.GetReviewById;

internal sealed class GetReviewByIdQueryHandler
    : IRequestHandler<GetReviewByIdQuery, Result<ReviewDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetReviewByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ReviewDetailDto>> Handle(
        GetReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetWithIssuesByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
            return Result.Failure<ReviewDetailDto>("Review not found.");

        var pr = await _unitOfWork.PullRequests.GetByIdAsync(review.PullRequestId, cancellationToken);
        if (pr is null)
            return Result.Failure<ReviewDetailDto>("Review not found.");

        var repo = await _unitOfWork.Repositories.GetByIdAsync(pr.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure<ReviewDetailDto>("Review not found.");

        var userId = _currentUserService.GetClerkUserId();
        if (repo.ClerkUserId != userId)
            return Result.Failure<ReviewDetailDto>("Review not found.");

        var issues = review.Issues
            .Select(i => new ReviewIssueDto(
                i.Id,
                i.Severity.ToString(),
                i.Category.ToString(),
                i.FilePath,
                i.LineNumber,
                i.Description,
                i.Suggestion))
            .ToList()
            .AsReadOnly();

        var dto = new ReviewDetailDto(
            review.Id,
            pr.Id,
            pr.GitHubPrNumber,
            pr.Title,
            repo.FullName,
            review.TechDebtScore,
            review.Summary,
            review.Status.ToString(),
            review.ProcessingTimeMs,
            review.CreatedAt,
            issues);

        return Result.Success(dto);
    }
}
