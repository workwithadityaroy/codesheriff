using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Members.Queries.GetRepositoryMembers;

public sealed record GetRepositoryMembersQuery(Guid RepositoryId)
    : IRequest<Result<IReadOnlyList<MemberDto>>>;
