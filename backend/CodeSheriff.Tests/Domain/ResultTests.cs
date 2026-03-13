using CodeSheriff.Domain.Common;
using FluentAssertions;

namespace CodeSheriff.Tests.Domain;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        const string errorMessage = "Something went wrong";

        var result = Result.Failure(errorMessage);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void SuccessOfT_ShouldHoldValue()
    {
        const int expectedValue = 42;

        var result = Result.Success(expectedValue);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void FailureOfT_AccessingValue_ShouldThrowInvalidOperationException()
    {
        var result = Result.Failure<int>("error");

        var action = () => _ = result.Value;

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value of a failed Result.");
    }

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(5);

        var mapped = result.Map(v => v * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var result = Result.Failure<int>("original error");

        var mapped = result.Map(v => v * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("original error");
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSuccessResult()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_WithNullError_ShouldThrowInvalidOperationException()
    {
        var action = () => Result.Failure(null!);

        action.Should().Throw<InvalidOperationException>();
    }
}
