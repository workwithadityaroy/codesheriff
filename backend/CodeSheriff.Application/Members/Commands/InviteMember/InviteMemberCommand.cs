using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.InviteMember;

public sealed record InviteMemberCommand(Guid RepositoryId, string Email, string Role)
    : IRequest<Result<InviteMemberResult>>;

public sealed record InviteMemberResult(Guid MemberId, string InviteToken);
