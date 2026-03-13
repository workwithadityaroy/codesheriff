using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Reports.Commands.SendWeeklyReport;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Repository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Tests.Application;

public sealed class SendWeeklyReportCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepositoryRepository> _repositoryRepo = new();
    private readonly Mock<IPullRequestRepository> _pullRequestRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IEmailService> _emailService = new();

    private readonly SendWeeklyReportCommandHandler _handler;

    private const string ClerkUserId = "user_abc";
    private const string UserEmail = "test@example.com";
    private const string DisplayName = "Test User";

    public SendWeeklyReportCommandHandlerTests()
    {
        _unitOfWork.SetupGet(u => u.Repositories).Returns(_repositoryRepo.Object);
        _unitOfWork.SetupGet(u => u.PullRequests).Returns(_pullRequestRepo.Object);
        _unitOfWork.SetupGet(u => u.Reviews).Returns(_reviewRepo.Object);

        _handler = new SendWeeklyReportCommandHandler(
            _unitOfWork.Object,
            _emailService.Object,
            NullLogger<SendWeeklyReportCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_NoRepositories_DoesNotSendEmail_ReturnsSuccess()
    {
        _repositoryRepo
            .Setup(r => r.GetActiveByClerkUserIdAsync(ClerkUserId, default))
            .ReturnsAsync(new List<Repository>().AsReadOnly());

        var result = await _handler.Handle(
            new SendWeeklyReportCommand(ClerkUserId, UserEmail, DisplayName), default);

        result.IsSuccess.Should().BeTrue();
        _emailService.Verify(
            e => e.SendWeeklyReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WeeklyReportData>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithReviewedPrsInLastWeek_SendsEmailWithCorrectData()
    {
        var repoId = Guid.NewGuid();
        var repo = Repository.Create(1L, "owner", "repo", "owner/repo", 1L, ClerkUserId);

        var pr = PullRequest.Create(repoId, 42, "Fix auth bug", "feature/auth", "main", "dev");
        pr.MarkAsReviewing();
        pr.MarkAsReviewed(); // Sets UpdatedAt = UtcNow (within 7 days)

        var review = Review.Create(pr.Id);
        review.MarkAsProcessing();
        review.Complete(45m, "Looks good overall.", "{}", 800);

        _repositoryRepo
            .Setup(r => r.GetActiveByClerkUserIdAsync(ClerkUserId, default))
            .ReturnsAsync(new List<Repository> { repo }.AsReadOnly());
        _pullRequestRepo
            .Setup(r => r.GetByRepositoryIdAsync(repo.Id, default))
            .ReturnsAsync(new List<PullRequest> { pr }.AsReadOnly());
        _reviewRepo
            .Setup(r => r.GetLatestWithIssuesByPullRequestIdAsync(pr.Id, default))
            .ReturnsAsync(review);

        WeeklyReportData? capturedData = null;
        _emailService
            .Setup(e => e.SendWeeklyReportAsync(UserEmail, DisplayName, It.IsAny<WeeklyReportData>(), default))
            .Callback<string, string, WeeklyReportData, CancellationToken>((_, _, data, _) => capturedData = data)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new SendWeeklyReportCommand(ClerkUserId, UserEmail, DisplayName), default);

        result.IsSuccess.Should().BeTrue();
        _emailService.Verify(
            e => e.SendWeeklyReportAsync(UserEmail, DisplayName, It.IsAny<WeeklyReportData>(), default),
            Times.Once);

        capturedData.Should().NotBeNull();
        capturedData!.TotalReviewed.Should().Be(1);
        capturedData.AverageTechDebtScore.Should().Be(45m);
        capturedData.Repos.Should().HaveCount(1);
        capturedData.Repos[0].FullName.Should().Be("owner/repo");
    }
}
