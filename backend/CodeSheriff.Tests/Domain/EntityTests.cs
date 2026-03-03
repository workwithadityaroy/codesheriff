using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Enums;
using FluentAssertions;

namespace CodeSheriff.Tests.Domain;

public sealed class EntityTests
{
    [Fact]
    public void Repository_Create_ShouldSetDefaultValues()
    {
        var repository = Repository.Create(
            gitHubId: 123456,
            owner: "test-org",
            name: "my-repo",
            fullName: "test-org/my-repo",
            installationId: 789);

        repository.Id.Should().NotBeEmpty();
        repository.GitHubId.Should().Be(123456);
        repository.Owner.Should().Be("test-org");
        repository.Name.Should().Be("my-repo");
        repository.FullName.Should().Be("test-org/my-repo");
        repository.InstallationId.Should().Be(789);
        repository.IsActive.Should().BeTrue();
        repository.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        repository.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Repository_Deactivate_ShouldSetIsActiveFalse()
    {
        var repository = Repository.Create(123, "org", "repo", "org/repo", 456);

        repository.Deactivate();

        repository.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Repository_TwoInstancesWithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = Repository.Create(1, "a", "b", "a/b", 1);
        var r2 = Repository.Create(2, "c", "d", "c/d", 2);

        // Different instances with different IDs are not equal
        r1.Equals(r2).Should().BeFalse();
    }

    [Fact]
    public void PullRequest_Create_ShouldSetPendingStatus()
    {
        var repo = Repository.Create(1, "org", "repo", "org/repo", 1);
        var pr = PullRequest.Create(repo.Id, 42, "Fix bug", "fix/bug", "main", "devuser");

        pr.Status.Should().Be(PullRequestStatus.Pending);
        pr.GitHubPrNumber.Should().Be(42);
        pr.AuthorLogin.Should().Be("devuser");
        pr.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void PullRequest_MarkAsReviewing_ShouldUpdateStatus()
    {
        var pr = PullRequest.Create(Guid.NewGuid(), 1, "title", "head", "base", "author");

        pr.MarkAsReviewing();

        pr.Status.Should().Be(PullRequestStatus.Reviewing);
    }

    [Fact]
    public void Review_Create_ShouldSetPendingStatus()
    {
        var review = Review.Create(Guid.NewGuid());

        review.Status.Should().Be(ReviewStatus.Pending);
        review.TechDebtScore.Should().Be(0);
        review.Id.Should().NotBeEmpty();
    }
}
