using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Members.Queries.GetRepositoryMembers;

internal sealed class GetRepositoryMembersQueryHandler
    : IRequestHandler<GetRepositoryMembersQuery, Result<IReadOnlyList<MemberDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetRepositoryMembersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<MemberDto>>> Handle(
        GetRepositoryMembersQuery request, CancellationToken cancellationToken)
    {
        var repo = await _unitOfWork.Repositories.GetByIdAsync(request.RepositoryId, cancellationToken);
        if (repo is null)
            return Result.Failure<IReadOnlyList<MemberDto>>("Repository not found.");

        var userId = _currentUserService.GetClerkUserId();
        var isOwner = repo.ClerkUserId == userId;
        var isMember = !isOwner && await _unitOfWork.Members.IsMemberAsync(repo.Id, userId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Failure<IReadOnlyList<MemberDto>>("Repository not found.");

        var members = await _unitOfWork.Members.GetByRepositoryIdAsync(repo.Id, cancellationToken);

        // Prepend the owner as a synthetic entry (Id = Guid.Empty signals no-delete)
        var result = new List<MemberDto>(members.Count + 1)
        {
            new(Guid.Empty, repo.Id, repo.ClerkUserId, string.Empty, "Owner", true, repo.CreatedAt),
        };

        result.AddRange(members.Select(m => new MemberDto(
            m.Id, m.RepositoryId, m.ClerkUserId, m.InvitedEmail,
            m.Role.ToString(), m.AcceptedAt.HasValue, m.CreatedAt)));

        return Result.Success<IReadOnlyList<MemberDto>>(result.AsReadOnly());
    }
}
