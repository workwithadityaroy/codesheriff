using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Enums;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Members.Commands.InviteMember;

internal sealed class InviteMemberCommandHandler
    : IRequestHandler<InviteMemberCommand, Result<InviteMemberResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public InviteMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<InviteMemberResult>> Handle(
        InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var repo = await _unitOfWork.Repositories.GetByIdAsync(request.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure<InviteMemberResult>("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        if (repo.ClerkUserId != userId)
            return Result.Failure<InviteMemberResult>("Only the repository owner can invite members.");

        if (!Enum.TryParse<MemberRole>(request.Role, ignoreCase: true, out var role) || role == MemberRole.Owner)
            return Result.Failure<InviteMemberResult>("Role must be 'Reviewer' or 'Viewer'.");

        var normalizedEmail = request.Email.ToLowerInvariant();
        var existing = await _unitOfWork.Members.GetByRepositoryIdAsync(repo.Id, cancellationToken);
        if (existing.Any(m => m.InvitedEmail == normalizedEmail))
            return Result.Failure<InviteMemberResult>("This email has already been invited.");

        var member = RepositoryMember.CreateInvite(repo.Id, normalizedEmail, role);
        await _unitOfWork.Members.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new InviteMemberResult(member.Id, member.InviteToken!));
    }
}
