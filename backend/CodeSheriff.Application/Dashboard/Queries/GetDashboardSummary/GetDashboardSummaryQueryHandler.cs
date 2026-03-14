using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Dashboard.Queries.GetDashboardSummary;

internal sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetClerkUserId();
        var repositories = await _unitOfWork.Repositories.GetAccessibleByClerkUserIdAsync(userId, cancellationToken);

        var totalRepositories = repositories.Count;
        var totalPrsReviewed = 0;
        var techDebtScores = new List<decimal>();
        var criticalIssueCount = 0;

        foreach (var repo in repositories)
        {
            var pullRequests = await _unitOfWork.PullRequests.GetByRepositoryIdAsync(repo.Id, cancellationToken);

            foreach (var pr in pullRequests.Where(p => p.Status == PullRequestStatus.Reviewed))
            {
                totalPrsReviewed++;

                var review = await _unitOfWork.Reviews.GetLatestWithIssuesByPullRequestIdAsync(pr.Id, cancellationToken);

                if (review is null) continue;

                techDebtScores.Add(review.TechDebtScore);
                criticalIssueCount += review.Issues.Count(i => i.Severity == IssueSeverity.Critical);
            }
        }

        var averageTechDebtScore = techDebtScores.Count > 0
            ? Math.Round(techDebtScores.Average(), 1)
            : 0m;

        return Result.Success(new DashboardSummaryDto(
            totalRepositories,
            totalPrsReviewed,
            averageTechDebtScore,
            criticalIssueCount));
    }
}
