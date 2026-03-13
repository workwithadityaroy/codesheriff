using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Reviews.Queries.GetReviewById;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Repository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Tests.Application;

public sealed class GetReviewByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepositoryRepository> _repositoryRepo = new();
    private readonly Mock<IPullRequestRepository> _pullRequestRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private readonly GetReviewByIdQueryHandler _handler;

    private static readonly Guid RepoId = Guid.NewGuid();
    private static readonly Guid ReviewId = Guid.NewGuid();
    private const string UserId = "user_123";

    public GetReviewByIdQueryHandlerTests()
    {
        _unitOfWork.SetupGet(u => u.Repositories).Returns(_repositoryRepo.Object);
        _unitOfWork.SetupGet(u => u.PullRequests).Returns(_pullRequestRepo.Object);
        _unitOfWork.SetupGet(u => u.Reviews).Returns(_reviewRepo.Object);
        _currentUserService.Setup(s => s.GetClerkUserId()).Returns(UserId);

        _handler = new GetReviewByIdQueryHandler(
            _unitOfWork.Object,
            _currentUserService.Object);
    }

    private static Repository MakeRepository(string clerkUserId = UserId)
        => Repository.Create(12345L, "owner", "repo", "owner/repo", 99L, clerkUserId);

    [Fact]
    public async Task Handle_ReviewNotFound_ReturnsFailure()
    {
        _reviewRepo
            .Setup(r => r.GetWithIssuesByIdAsync(ReviewId, default))
            .ReturnsAsync((Review?)null);

        var result = await _handler.Handle(new GetReviewByIdQuery(ReviewId), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RepoBelongsToDifferentUser_ReturnsFailure()
    {
        var pr = PullRequest.Create(RepoId, 1, "PR title", "feature", "main", "author");
        var review = Review.Create(pr.Id);
        var repo = MakeRepository(clerkUserId: "other_user");

        _reviewRepo.Setup(r => r.GetWithIssuesByIdAsync(ReviewId, default)).ReturnsAsync(review);
        _pullRequestRepo.Setup(r => r.GetByIdAsync(review.PullRequestId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);

        var result = await _handler.Handle(new GetReviewByIdQuery(ReviewId), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsMappedReviewDetail()
    {
        var pr = PullRequest.Create(RepoId, 5, "Add feature", "feat/x", "main", "dev");
        var review = Review.Create(pr.Id);
        review.MarkAsProcessing();
        review.Complete(72m, "High debt detected.", "{}", 1500);
        var repo = MakeRepository();

        _reviewRepo.Setup(r => r.GetWithIssuesByIdAsync(ReviewId, default)).ReturnsAsync(review);
        _pullRequestRepo.Setup(r => r.GetByIdAsync(review.PullRequestId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);

        var result = await _handler.Handle(new GetReviewByIdQuery(ReviewId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TechDebtScore.Should().Be(72m);
        result.Value.PullRequestTitle.Should().Be("Add feature");
        result.Value.RepositoryFullName.Should().Be("owner/repo");
    }
}
