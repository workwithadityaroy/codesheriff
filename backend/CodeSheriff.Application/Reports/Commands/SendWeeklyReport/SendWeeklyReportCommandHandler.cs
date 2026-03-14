using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Application.Reports.Commands.SendWeeklyReport;

internal sealed class SendWeeklyReportCommandHandler
    : IRequestHandler<SendWeeklyReportCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendWeeklyReportCommandHandler> _logger;

    public SendWeeklyReportCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<SendWeeklyReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SendWeeklyReportCommand request,
        CancellationToken cancellationToken)
    {
        var repos = await _unitOfWork.Repositories
            .GetActiveByClerkUserIdAsync(request.ClerkUserId, cancellationToken);

        if (repos.Count == 0)
        {
            _logger.LogInformation(
                "No repositories for user {ClerkUserId} — skipping weekly report.", request.ClerkUserId);
            return Result.Success();
        }

        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        var repoItems = new List<RepoReportItem>();
        int totalReviewed = 0;
        decimal totalDebtScore = 0;
        int criticalIssueCount = 0;
        int reviewedWithScore = 0;

        foreach (var repo in repos)
        {
            var pullRequests = await _unitOfWork.PullRequests
                .GetByRepositoryIdAsync(repo.Id, cancellationToken);

            var reviewedPrs = pullRequests
                .Where(pr => pr.Status == PullRequestStatus.Reviewed && pr.UpdatedAt >= cutoff)
                .ToList();

            if (reviewedPrs.Count == 0)
                continue;

            decimal repoTotalScore = 0;
            int repoReviewedCount = 0;

            foreach (var pr in reviewedPrs)
            {
                var review = await _unitOfWork.Reviews
                    .GetLatestWithIssuesByPullRequestIdAsync(pr.Id, cancellationToken);

                if (review is null || review.Status != ReviewStatus.Completed)
                    continue;

                repoTotalScore += review.TechDebtScore;
                repoReviewedCount++;
                totalReviewed++;
                totalDebtScore += review.TechDebtScore;
                reviewedWithScore++;

                criticalIssueCount += review.Issues
                    .Count(i => i.Severity == IssueSeverity.Critical);
            }

            if (repoReviewedCount > 0)
            {
                repoItems.Add(new RepoReportItem(
                    repo.FullName,
                    repoReviewedCount,
                    Math.Round(repoTotalScore / repoReviewedCount, 1)));
            }
        }

        if (totalReviewed == 0)
        {
            _logger.LogInformation(
                "No reviewed PRs this week for user {ClerkUserId} — skipping email.", request.ClerkUserId);
            return Result.Success();
        }

        var data = new WeeklyReportData(
            TotalReviewed: totalReviewed,
            AverageTechDebtScore: reviewedWithScore > 0
                ? Math.Round(totalDebtScore / reviewedWithScore, 1)
                : 0,
            CriticalIssueCount: criticalIssueCount,
            Repos: repoItems);

        await _emailService.SendWeeklyReportAsync(
            request.UserEmail, request.DisplayName, data, cancellationToken);

        _logger.LogInformation(
            "Weekly report sent to {Email} — {Count} PRs reviewed.", request.UserEmail, totalReviewed);

        return Result.Success();
    }

}
