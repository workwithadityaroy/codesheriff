using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.RemoveMember;

public sealed record RemoveMemberCommand(Guid RepositoryId, Guid MemberId) : IRequest<Result>;
