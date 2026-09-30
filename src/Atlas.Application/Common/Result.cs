namespace Atlas.Application.Common;

/// <summary>Outcome of a command: success, or a list of errors.</summary>
public class Result
{
    protected Result(IReadOnlyList<Error> errors) => Errors = errors;

    public IReadOnlyList<Error> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    /// <summary>All error messages joined for display.</summary>
    public string ErrorMessage => string.Join(" ", Errors.Select(e => e.Message));

    public static Result Success() => new([]);

    public static Result Failure(params Error[] errors) => Failure((IEnumerable<Error>)errors);

    public static Result Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("A failure needs at least one error.", nameof(errors));
        }

        return new Result(list);
    }

    public static Result<T> Success<T>(T value) => Result<T>.FromValue(value);

    public static Result<T> Failure<T>(params Error[] errors) => Result<T>.FromErrors(errors);

    public static Result<T> Failure<T>(IEnumerable<Error> errors) => Result<T>.FromErrors(errors);
}

/// <summary>Outcome of a command that produces a value on success.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, IReadOnlyList<Error> errors)
        : base(errors) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    internal static Result<T> FromValue(T value) => new(value, []);

    internal static Result<T> FromErrors(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return list.Count == 0
            ? throw new ArgumentException("A failure needs at least one error.", nameof(errors))
            : new Result<T>(default, list);
    }
}
