namespace TopLab.Application.Common.Results;

/// <summary>
/// Common contract for <see cref="Result"/> and <see cref="Result{T}"/> so that
/// pipeline behaviors can inspect success/failure uniformly (ADR-0008, ADR-0009).
/// </summary>
public interface IResult
{
    bool IsSuccess { get; }

    IReadOnlyList<Error> Errors { get; }

    Error? Error { get; }
}

/// <summary>
/// Represents the outcome of a use case that returns no value.
/// Validation failures collect every violated rule (not only the first) so a caller
/// can surface them all at once (Coding Standards §6.2).
/// </summary>
public class Result : IResult
{
    public bool IsSuccess { get; protected init; }

    public IReadOnlyList<Error> Errors { get; protected init; } = [];

    public Error? Error => Errors.Count > 0 ? Errors[0] : null;

    protected Result()
    {
    }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(Error error) => new() { IsSuccess = false, Errors = [error] };

    public static Result Failure(IReadOnlyList<Error> errors)
        => new() { IsSuccess = false, Errors = errors };
}

/// <summary>
/// Represents the outcome of a use case that returns a value of type <typeparamref name="T"/>.
/// </summary>
public class Result<T> : IResult
{
    public bool IsSuccess { get; protected init; }

    public IReadOnlyList<Error> Errors { get; protected init; } = [];

    public Error? Error => Errors.Count > 0 ? Errors[0] : null;

    public T? Value { get; protected init; }

    protected Result()
    {
    }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(Error error) => new() { IsSuccess = false, Errors = [error] };

    public static Result<T> Failure(IReadOnlyList<Error> errors)
        => new() { IsSuccess = false, Errors = errors };
}
