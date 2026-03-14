using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.AcceptInvite;

internal sealed class AcceptInviteCommandHandler : IRequestHandler<AcceptInviteCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AcceptInviteCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.Members.GetByTokenAsync(request.Token, cancellationToken);
        if (member is null)
            return Result.Failure<Guid>("Invite not found or already accepted.");

        if (member.AcceptedAt.HasValue)
            return Result.Failure<Guid>("This invite has already been accepted.");

        var clerkUserId = _currentUserService.GetClerkUserId();
        member.Accept(clerkUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(member.RepositoryId);
    }
}
