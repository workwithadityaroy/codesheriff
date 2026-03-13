using CodeSheriff.Application.Common.Models;
using CodeSheriff.Domain.Common;

namespace CodeSheriff.Application.Common.Interfaces;

public interface IAiReviewService
{
    Task<Result<AiReviewResult>> ReviewPullRequestAsync(
        string diff,
        string repoFullName,
        int prNumber,
        string prTitle,
        CancellationToken cancellationToken = default);
}
