using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Reviews.Commands.AnalyzePullRequest;

public sealed record AnalyzePullRequestCommand(
    Guid PullRequestId,
    long InstallationId,
    string Owner,
    string RepoName,
    int GitHubPrNumber) : IRequest<Result<Guid>>;
