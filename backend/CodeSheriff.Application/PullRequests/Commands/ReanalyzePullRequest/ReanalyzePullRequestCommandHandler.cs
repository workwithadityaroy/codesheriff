using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;
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

        var hasActive = await _unitOfWork.Reviews.HasActiveReviewAsync(
            request.PullRequestId, cancellationToken);

        if (hasActive)
            return Result.Failure("A review is already in progress for this pull request.");

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
