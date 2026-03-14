using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Application.Reviews.Commands.AnalyzePullRequest;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Repository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Tests.Application;

public sealed class AnalyzePullRequestCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPullRequestRepository> _pullRequestRepo = new();
    private readonly Mock<IRepositoryRepository> _repositoryRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IGitProviderFactory> _gitProviderFactory = new();
    private readonly Mock<IGitProvider> _gitProvider = new();
    private readonly Mock<IAiReviewService> _aiReviewService = new();

    private readonly AnalyzePullRequestCommandHandler _handler;

    private static readonly Guid PrId = Guid.NewGuid();
    private static readonly Guid RepoId = Guid.NewGuid();

    public AnalyzePullRequestCommandHandlerTests()
    {
        _unitOfWork.SetupGet(u => u.PullRequests).Returns(_pullRequestRepo.Object);
        _unitOfWork.SetupGet(u => u.Repositories).Returns(_repositoryRepo.Object);
        _unitOfWork.SetupGet(u => u.Reviews).Returns(_reviewRepo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _gitProvider.SetupGet(p => p.ProviderType).Returns(GitProvider.GitHub);
        _gitProviderFactory
            .Setup(f => f.GetProvider(It.IsAny<GitProvider>()))
            .Returns(_gitProvider.Object);

        _handler = new AnalyzePullRequestCommandHandler(
            _unitOfWork.Object,
            _gitProviderFactory.Object,
            _aiReviewService.Object,
            NullLogger<AnalyzePullRequestCommandHandler>.Instance);
    }

    private static PullRequest MakePullRequest()
        => PullRequest.Create(RepoId, 42, "Test PR", "feature", "main", "author");

    private static Repository MakeRepository()
        => Repository.Create(12345L, "owner", "repo", "owner/repo", 99L, "user_123");

    [Fact]
    public async Task Handle_PullRequestNotFound_ReturnsFailure()
    {
        _pullRequestRepo
            .Setup(r => r.GetByIdAsync(PrId, default))
            .ReturnsAsync((PullRequest?)null);

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        _reviewRepo.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_GitDiffFails_ReviewAndPrMarkedFailed()
    {
        var pr = MakePullRequest();
        var repo = MakeRepository();

        _pullRequestRepo.Setup(r => r.GetByIdAsync(PrId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);
        _reviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>(), default)).Returns(Task.CompletedTask);

        _gitProvider
            .Setup(p => p.GetPullRequestDiffAsync(repo, 42, default))
            .ReturnsAsync(Result.Failure<string>("Diff error"));

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        pr.Status.Should().Be(PullRequestStatus.Failed);
    }

    [Fact]
    public async Task Handle_AiServiceFails_ReviewAndPrMarkedFailed()
    {
        var pr = MakePullRequest();
        var repo = MakeRepository();

        _pullRequestRepo.Setup(r => r.GetByIdAsync(PrId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);
        _reviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>(), default)).Returns(Task.CompletedTask);

        _gitProvider
            .Setup(p => p.GetPullRequestDiffAsync(repo, 42, default))
            .ReturnsAsync(Result.Success("diff content"));

        _aiReviewService
            .Setup(s => s.ReviewPullRequestAsync("diff content", "owner/repo", 42, "Test PR", default))
            .ReturnsAsync(Result.Failure<AiReviewResult>("AI error"));

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        pr.Status.Should().Be(PullRequestStatus.Failed);
    }

    [Fact]
    public async Task Handle_HappyPath_ReviewCompletedWithCorrectScore()
    {
        var pr = MakePullRequest();
        var repo = MakeRepository();

        Review? capturedReview = null;
        _reviewRepo
            .Setup(r => r.AddAsync(It.IsAny<Review>(), default))
            .Callback<Review, CancellationToken>((r, _) => capturedReview = r)
            .Returns(Task.CompletedTask);

        _pullRequestRepo.Setup(r => r.GetByIdAsync(PrId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);

        _gitProvider
            .Setup(p => p.GetPullRequestDiffAsync(repo, 42, default))
            .ReturnsAsync(Result.Success("diff content"));

        _aiReviewService
            .Setup(s => s.ReviewPullRequestAsync("diff content", "owner/repo", 42, "Test PR", default))
            .ReturnsAsync(Result.Success(new AiReviewResult(42m, "Looks good.", "{}", [])));

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        capturedReview.Should().NotBeNull();
        capturedReview!.Status.Should().Be(ReviewStatus.Completed);
        capturedReview.TechDebtScore.Should().Be(42m);
    }

    [Fact]
    public async Task Handle_HappyPath_ReviewIssuesCreatedForEachAiIssue()
    {
        var pr = MakePullRequest();
        var repo = MakeRepository();
        var aiIssues = new List<AiReviewIssue>
        {
            new(IssueSeverity.Warning, IssueCategory.CodeSmell, "Foo.cs", 10, "Bad naming", "Rename it"),
            new(IssueSeverity.Error, IssueCategory.Security, "Bar.cs", null, "SQL injection", "Use params")
        };

        Review? capturedReview = null;
        _reviewRepo
            .Setup(r => r.AddAsync(It.IsAny<Review>(), default))
            .Callback<Review, CancellationToken>((r, _) => capturedReview = r)
            .Returns(Task.CompletedTask);

        _pullRequestRepo.Setup(r => r.GetByIdAsync(PrId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);

        _gitProvider
            .Setup(p => p.GetPullRequestDiffAsync(repo, 42, default))
            .ReturnsAsync(Result.Success("diff content"));

        _aiReviewService
            .Setup(s => s.ReviewPullRequestAsync("diff content", "owner/repo", 42, "Test PR", default))
            .ReturnsAsync(Result.Success(new AiReviewResult(25m, "Summary.", "{}", aiIssues)));

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        await _handler.Handle(command, default);

        capturedReview.Should().NotBeNull();
        capturedReview!.Issues.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_HappyPath_PullRequestMarkedAsReviewed()
    {
        var pr = MakePullRequest();
        var repo = MakeRepository();

        _reviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>(), default)).Returns(Task.CompletedTask);
        _pullRequestRepo.Setup(r => r.GetByIdAsync(PrId, default)).ReturnsAsync(pr);
        _repositoryRepo.Setup(r => r.GetByIdAsync(pr.RepositoryId, default)).ReturnsAsync(repo);

        _gitProvider
            .Setup(p => p.GetPullRequestDiffAsync(repo, 42, default))
            .ReturnsAsync(Result.Success("diff content"));

        _aiReviewService
            .Setup(s => s.ReviewPullRequestAsync("diff content", "owner/repo", 42, "Test PR", default))
            .ReturnsAsync(Result.Success(new AiReviewResult(0m, "Clean code.", "{}", [])));

        var command = new AnalyzePullRequestCommand(PrId, 99L, "owner", "repo", 42);
        await _handler.Handle(command, default);

        pr.Status.Should().Be(PullRequestStatus.Reviewed);
    }
}
