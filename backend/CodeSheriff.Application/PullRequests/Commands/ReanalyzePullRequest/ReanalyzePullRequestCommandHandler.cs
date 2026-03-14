using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Commands.ReanalyzePullRequest;

internal sealed class ReanalyzePullRequestCommandHandler
    : IRequestHandler<ReanalyzePullRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewQueueService _reviewQueueService;
    private readonly ICurrentUserService _currentUserService;

    public ReanalyzePullRequestCommandHandler(
        IUnitOfWork unitOfWork,
        IReviewQueueService reviewQueueService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _reviewQueueService = reviewQueueService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        ReanalyzePullRequestCommand request,
        CancellationToken cancellationToken)
    {
        var pullRequest = await _unitOfWork.PullRequests.GetByIdAsync(
            request.PullRequestId, cancellationToken);

        if (pullRequest is null)
            return Result.Failure("Pull request not found.");

        var repository = await _unitOfWork.Repositories.GetByIdAsync(
            pullRequest.RepositoryId, cancellationToken);

        if (repository is null)
            return Result.Failure("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        if (repository.ClerkUserId != userId)
            return Result.Failure("Pull request not found.");

        // If there's an active review that has been stuck for > 10 minutes, force-fail it
        // so the user can re-trigger without being permanently blocked.
        const int staleThresholdMinutes = 10;
        var activeReview = await _unitOfWork.Reviews.GetLatestByPullRequestIdAsync(
            request.PullRequestId, cancellationToken);

        if (activeReview is not null
            && (activeReview.Status == ReviewStatus.Pending || activeReview.Status == ReviewStatus.Processing))
        {
            var ageMinutes = (DateTimeOffset.UtcNow - activeReview.CreatedAt).TotalMinutes;
            if (ageMinutes < staleThresholdMinutes)
                return Result.Failure("A review is already in progress. Please wait a moment.");

            // Stale — reset it so the user can retry
            activeReview.MarkAsFailed();
            pullRequest.MarkAsFailed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        pullRequest.MarkAsReviewing();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _reviewQueueService.EnqueueAsync(
            new ReviewJobMessage(
                pullRequest.Id,
                repository.InstallationId,
                repository.Owner,
                repository.Name,
                pullRequest.GitHubPrNumber),
            cancellationToken);

        return Result.Success();
    }
}
