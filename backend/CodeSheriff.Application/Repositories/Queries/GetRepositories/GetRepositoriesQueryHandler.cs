using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Repositories.Queries.GetRepositories;

internal sealed class GetRepositoriesQueryHandler
    : IRequestHandler<GetRepositoriesQuery, Result<IReadOnlyList<RepositoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetRepositoriesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<RepositoryDto>>> Handle(
        GetRepositoriesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetClerkUserId();
        var repositories = await _unitOfWork.Repositories.GetActiveByClerkUserIdAsync(userId, cancellationToken);

        var dtos = repositories
            .Select(r => new RepositoryDto(
                r.Id,
                r.GitHubId,
                r.Owner,
                r.Name,
                r.FullName,
                r.IsActive,
                r.CreatedAt,
                r.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<RepositoryDto>>(dtos);
    }
}
