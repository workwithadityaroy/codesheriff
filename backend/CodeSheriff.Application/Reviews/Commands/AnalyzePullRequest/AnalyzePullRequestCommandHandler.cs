using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Application.Reviews.Commands.AnalyzePullRequest;

internal sealed class AnalyzePullRequestCommandHandler
    : IRequestHandler<AnalyzePullRequestCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGitHubService _gitHubService;
    private readonly IAiReviewService _aiReviewService;
    private readonly ILogger<AnalyzePullRequestCommandHandler> _logger;

    public AnalyzePullRequestCommandHandler(
        IUnitOfWork unitOfWork,
        IGitHubService gitHubService,
        IAiReviewService aiReviewService,
        ILogger<AnalyzePullRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _gitHubService = gitHubService;
        _aiReviewService = aiReviewService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        AnalyzePullRequestCommand request,
        CancellationToken cancellationToken)
    {
        var pullRequest = await _unitOfWork.PullRequests.GetByIdAsync(
            request.PullRequestId, cancellationToken);

        if (pullRequest is null)
            return Result.Failure<Guid>($"PullRequest {request.PullRequestId} not found.");

        var repository = await _unitOfWork.Repositories.GetByIdAsync(
            pullRequest.RepositoryId, cancellationToken);

        if (repository is null)
            return Result.Failure<Guid>($"Repository {pullRequest.RepositoryId} not found.");

        var review = Review.Create(pullRequest.Id);
        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        review.MarkAsProcessing();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Starting review for PR #{PrNumber} in {RepoFullName}",
            request.GitHubPrNumber, repository.FullName);

        var diffResult = await _gitHubService.GetPullRequestDiffAsync(
            request.InstallationId, request.Owner, request.RepoName,
            request.GitHubPrNumber, cancellationToken);

        if (!diffResult.IsSuccess)
        {
            _logger.LogWarning("GitHub diff fetch failed: {Error}", diffResult.Error);
            review.MarkAsFailed();
            pullRequest.MarkAsFailed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<Guid>(diffResult.Error);
        }

        var aiResult = await _aiReviewService.ReviewPullRequestAsync(
            diffResult.Value!, repository.FullName,
            request.GitHubPrNumber, pullRequest.Title,
            cancellationToken);

        if (!aiResult.IsSuccess)
        {
            _logger.LogWarning("AI review failed: {Error}", aiResult.Error);
            review.MarkAsFailed();
            pullRequest.MarkAsFailed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<Guid>(aiResult.Error);
        }

        var aiReviewResult = aiResult.Value!;
        var processingTimeMs = (int)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;

        review.Complete(
            aiReviewResult.TechDebtScore,
            aiReviewResult.Summary,
            aiReviewResult.RawResponse,
            processingTimeMs);

        pullRequest.MarkAsReviewed();

        foreach (var issue in aiReviewResult.Issues)
        {
            var reviewIssue = ReviewIssue.Create(
                review.Id,
                issue.Severity,
                issue.Category,
                issue.FilePath,
                issue.LineNumber,
                issue.Description,
                issue.Suggestion);

            review.Issues.Add(reviewIssue);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Review {ReviewId} completed for PR #{PrNumber} — score: {Score}",
            review.Id, request.GitHubPrNumber, aiReviewResult.TechDebtScore);

        return Result.Success(review.Id);
    }
}
