using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.PullRequests.Queries.GetPullRequestsByRepository;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Repository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Tests.Application;

public sealed class GetPullRequestsByRepositoryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepositoryRepository> _repositoryRepo = new();
    private readonly Mock<IPullRequestRepository> _pullRequestRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IRepositoryMemberRepository> _membersRepo = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private readonly GetPullRequestsByRepositoryQueryHandler _handler;

    private static readonly Guid RepoId = Guid.NewGuid();
    private const string UserId = "user_123";

    public GetPullRequestsByRepositoryQueryHandlerTests()
    {
        _unitOfWork.SetupGet(u => u.Repositories).Returns(_repositoryRepo.Object);
        _unitOfWork.SetupGet(u => u.PullRequests).Returns(_pullRequestRepo.Object);
        _unitOfWork.SetupGet(u => u.Reviews).Returns(_reviewRepo.Object);
        _unitOfWork.SetupGet(u => u.Members).Returns(_membersRepo.Object);
        _membersRepo.Setup(m => m.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _currentUserService.Setup(s => s.GetClerkUserId()).Returns(UserId);

        _handler = new GetPullRequestsByRepositoryQueryHandler(
            _unitOfWork.Object,
            _currentUserService.Object);
    }

    private static Repository MakeRepository(string clerkUserId = UserId)
        => Repository.Create(12345L, "owner", "repo", "owner/repo", 99L, clerkUserId);

    [Fact]
    public async Task Handle_RepositoryNotFound_ReturnsFailure()
    {
        _repositoryRepo
            .Setup(r => r.GetByIdAsync(RepoId, default))
            .ReturnsAsync((Repository?)null);

        var result = await _handler.Handle(new GetPullRequestsByRepositoryQuery(RepoId), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RepositoryBelongsToDifferentUser_ReturnsFailure()
    {
        var repo = MakeRepository(clerkUserId: "other_user");
        _repositoryRepo
            .Setup(r => r.GetByIdAsync(RepoId, default))
            .ReturnsAsync(repo);

        var result = await _handler.Handle(new GetPullRequestsByRepositoryQuery(RepoId), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsMappedPullRequestList()
    {
        var repo = MakeRepository();
        var pr = PullRequest.Create(RepoId, 7, "Fix bug", "fix/bug", "main", "dev");

        _repositoryRepo.Setup(r => r.GetByIdAsync(RepoId, default)).ReturnsAsync(repo);
        _pullRequestRepo
            .Setup(r => r.GetByRepositoryIdAsync(RepoId, default))
            .ReturnsAsync(new List<PullRequest> { pr }.AsReadOnly());
        _reviewRepo
            .Setup(r => r.GetLatestWithIssuesByPullRequestIdAsync(pr.Id, default))
            .ReturnsAsync((Review?)null);

        var result = await _handler.Handle(new GetPullRequestsByRepositoryQuery(RepoId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].GitHubPrNumber.Should().Be(7);
    }
}
