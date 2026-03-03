namespace CodeSheriff.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null) => _value = value;
    private Result(string error) : base(false, error) => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(Value)) : Result<TOut>.Failure(Error);

    public static implicit operator Result<T>(T value) => Success(value);
}
