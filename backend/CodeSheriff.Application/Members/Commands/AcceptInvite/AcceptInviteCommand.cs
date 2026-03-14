using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.AcceptInvite;

public sealed record AcceptInviteCommand(string Token) : IRequest<Result<Guid>>;
