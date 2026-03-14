using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.RemoveMember;

internal sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var repo = await _unitOfWork.Repositories.GetByIdAsync(request.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        if (repo.ClerkUserId != userId)
            return Result.Failure("Only the repository owner can remove members.");

        var member = await _unitOfWork.Members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.RepositoryId != request.RepositoryId)
            return Result.Failure("Member not found.");

        _unitOfWork.Members.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
