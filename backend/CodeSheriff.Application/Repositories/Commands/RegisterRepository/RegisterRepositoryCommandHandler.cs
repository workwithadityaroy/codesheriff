using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Entities;
using CodeSheriff.Domain.Interfaces;
using MediatR;

namespace CodeSheriff.Application.Repositories.Commands.RegisterRepository;

internal sealed class RegisterRepositoryCommandHandler
    : IRequestHandler<RegisterRepositoryCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RegisterRepositoryCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        RegisterRepositoryCommand request,
        CancellationToken cancellationToken)
    {
        var clerkUserId = _currentUserService.GetClerkUserId();

        var existing = await _unitOfWork.Repositories.GetByGitHubIdAsync(request.GitHubId, cancellationToken);

        if (existing is not null)
        {
            if (existing.ClerkUserId == clerkUserId)
                return Result.Success(existing.Id);

            return Result.Failure<Guid>("Repository is already registered.");
        }

        var repository = Repository.Create(
            request.GitHubId,
            request.Owner,
            request.Name,
            request.FullName,
            request.InstallationId,
            clerkUserId);

        await _unitOfWork.Repositories.AddAsync(repository, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(repository.Id);
    }
}
