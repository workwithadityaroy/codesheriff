using CodeSheriff.Application.Repositories.Commands.RegisterRepository;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CodeSheriff.Tests.Application;

public sealed class RegisterRepositoryCommandValidatorTests
{
    private readonly RegisterRepositoryCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyOwner_ShouldFail()
    {
        var command = new RegisterRepositoryCommand(1, string.Empty, "repo", "org/repo", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Owner);
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var command = new RegisterRepositoryCommand(1, "org", string.Empty, "org/repo", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyFullName_ShouldFail()
    {
        var command = new RegisterRepositoryCommand(1, "org", "repo", string.Empty, 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_GitHubIdZero_ShouldFail()
    {
        var command = new RegisterRepositoryCommand(0, "org", "repo", "org/repo", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.GitHubId);
    }

    [Fact]
    public void Validate_InstallationIdZero_ShouldFail()
    {
        var command = new RegisterRepositoryCommand(1, "org", "repo", "org/repo", 0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InstallationId);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new RegisterRepositoryCommand(123456, "test-org", "my-repo", "test-org/my-repo", 789);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
