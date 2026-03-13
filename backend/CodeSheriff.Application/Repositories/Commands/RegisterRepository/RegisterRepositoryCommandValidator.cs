using FluentValidation;

namespace CodeSheriff.Application.Repositories.Commands.RegisterRepository;

public sealed class RegisterRepositoryCommandValidator : AbstractValidator<RegisterRepositoryCommand>
{
    public RegisterRepositoryCommandValidator()
    {
        RuleFor(x => x.Owner).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.GitHubId).GreaterThan(0);
        RuleFor(x => x.InstallationId).GreaterThan(0);
    }
}
