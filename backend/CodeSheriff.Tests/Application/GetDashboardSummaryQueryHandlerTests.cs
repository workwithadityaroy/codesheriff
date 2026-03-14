using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Dashboard.Queries.GetDashboardSummary;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Repository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Tests.Application;

public sealed class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepositoryRepository> _repositoryRepo = new();
    private readonly Mock<IPullRequestRepository> _pullRequestRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private readonly GetDashboardSummaryQueryHandler _handler;

    private const string UserId = "user_123";

    public GetDashboardSummaryQueryHandlerTests()
    {
        _unitOfWork.SetupGet(u => u.Repositories).Returns(_repositoryRepo.Object);
        _unitOfWork.SetupGet(u => u.PullRequests).Returns(_pullRequestRepo.Object);
        _unitOfWork.SetupGet(u => u.Reviews).Returns(_reviewRepo.Object);
        _currentUserService.Setup(s => s.GetClerkUserId()).Returns(UserId);

        _handler = new GetDashboardSummaryQueryHandler(
            _unitOfWork.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_NoRepositories_ReturnsAllZeros()
    {
        _repositoryRepo
            .Setup(r => r.GetAccessibleByClerkUserIdAsync(UserId, default))
            .ReturnsAsync(new List<Repository>().AsReadOnly());

        var result = await _handler.Handle(new GetDashboardSummaryQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRepositories.Should().Be(0);
        result.Value.TotalPrsReviewed.Should().Be(0);
        result.Value.AverageTechDebtScore.Should().Be(0m);
        result.Value.CriticalIssueCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithReviewedPrs_ReturnsCorrectAggregates()
    {
        var repoId = Guid.NewGuid();
        var repo = Repository.Create(1L, "owner", "repo", "owner/repo", 1L, UserId);

        var pr = PullRequest.Create(repoId, 1, "PR", "feature", "main", "dev");
        pr.MarkAsReviewing();
        pr.MarkAsReviewed();

        var review = Review.Create(pr.Id);
        review.MarkAsProcessing();
        review.Complete(60m, "Some debt.", "{}", 1000);

        _repositoryRepo
            .Setup(r => r.GetAccessibleByClerkUserIdAsync(UserId, default))
            .ReturnsAsync(new List<Repository> { repo }.AsReadOnly());
        _pullRequestRepo
            .Setup(r => r.GetByRepositoryIdAsync(repo.Id, default))
            .ReturnsAsync(new List<PullRequest> { pr }.AsReadOnly());
        _reviewRepo
            .Setup(r => r.GetLatestWithIssuesByPullRequestIdAsync(pr.Id, default))
            .ReturnsAsync(review);

        var result = await _handler.Handle(new GetDashboardSummaryQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRepositories.Should().Be(1);
        result.Value.TotalPrsReviewed.Should().Be(1);
        result.Value.AverageTechDebtScore.Should().Be(60m);
    }
}
