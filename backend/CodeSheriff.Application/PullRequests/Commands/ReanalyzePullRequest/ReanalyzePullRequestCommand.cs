using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.PullRequests.Commands.ReanalyzePullRequest;

public sealed record ReanalyzePullRequestCommand(Guid PullRequestId) : IRequest<Result>;
