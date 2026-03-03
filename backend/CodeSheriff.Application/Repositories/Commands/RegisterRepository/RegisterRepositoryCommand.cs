using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Repositories.Commands.RegisterRepository;

public sealed record RegisterRepositoryCommand(
    long GitHubId,
    string Owner,
    string Name,
    string FullName,
    long InstallationId
) : IRequest<Result<Guid>>;
