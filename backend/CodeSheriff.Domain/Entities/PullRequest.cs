using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Enums;

namespace CodeSheriff.Domain.Entities;

public class PullRequest : Entity
{
    public Guid RepositoryId { get; private set; }
    public int GitHubPrNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string HeadBranch { get; private set; } = string.Empty;
    public string BaseBranch { get; private set; } = string.Empty;
    public string AuthorLogin { get; private set; } = string.Empty;
    public PullRequestStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Repository Repository { get; private set; } = null!;
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();

    private PullRequest() { }

    public static PullRequest Create(
        Guid repositoryId,
        int gitHubPrNumber,
        string title,
        string headBranch,
        string baseBranch,
        string authorLogin)
    {
        return new PullRequest
        {
            RepositoryId = repositoryId,
            GitHubPrNumber = gitHubPrNumber,
            Title = title,
            HeadBranch = headBranch,
            BaseBranch = baseBranch,
            AuthorLogin = authorLogin,
            Status = PullRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsReviewing()
    {
        Status = PullRequestStatus.Reviewing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsReviewed()
    {
        Status = PullRequestStatus.Reviewed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = PullRequestStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
