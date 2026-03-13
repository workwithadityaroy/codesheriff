using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Repositories.Queries.GetRepositories;

public sealed record GetRepositoriesQuery : IRequest<Result<IReadOnlyList<RepositoryDto>>>;
