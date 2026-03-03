using CodeSheriff.Application.Repositories.Queries.GetRepositories;
using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Repositories.Queries.GetRepositoryById;

public sealed record GetRepositoryByIdQuery(Guid Id) : IRequest<Result<RepositoryDto>>;
