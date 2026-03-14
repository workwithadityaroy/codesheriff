using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Repositories.Queries.GetRepositories;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Repositories.Queries.GetRepositoryById;

internal sealed class GetRepositoryByIdQueryHandler
    : IRequestHandler<GetRepositoryByIdQuery, Result<RepositoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetRepositoryByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RepositoryDto>> Handle(
        GetRepositoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var repository = await _unitOfWork.Repositories.GetByIdAsync(request.Id, cancellationToken);

        if (repository is null)
            return Result.Failure<RepositoryDto>($"Repository with ID '{request.Id}' was not found.");

        var userId = _currentUserService.GetClerkUserId();
        var isOwner = repository.ClerkUserId == userId;
        var isMember = !isOwner && await _unitOfWork.Members.IsMemberAsync(repository.Id, userId, cancellationToken);
        if (!isOwner && !isMember)
            return Result.Failure<RepositoryDto>($"Repository with ID '{request.Id}' was not found.");

        var dto = new RepositoryDto(
            repository.Id,
            repository.GitHubId,
            repository.Owner,
            repository.Name,
            repository.FullName,
            repository.IsActive,
            repository.CreatedAt,
            repository.UpdatedAt);

        return Result.Success(dto);
    }
}
