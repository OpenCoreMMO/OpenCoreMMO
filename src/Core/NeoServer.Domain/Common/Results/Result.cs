namespace NeoServer.Domain.Common.Results;

public readonly ref struct Result
{
    public Result(InvalidOperation reason)
    {
        Reason = reason;
        IsNotApplicable = false;
    }

    private Result(InvalidOperation reason, bool notApplicable)
    {
        Reason = reason;
        IsNotApplicable = notApplicable;
    }

    public InvalidOperation Reason { get; }
    public bool IsNotApplicable { get; }

    public bool Succeeded => Reason == InvalidOperation.None;
    public bool Failed => !Succeeded;

    public static Result Success => new(InvalidOperation.None);
    public static Result NotPossible => new(InvalidOperation.NotPossible);
    public static Result NotApplicable => new(InvalidOperation.None, true);


    public static Result Fail(InvalidOperation invalidOperation)
    {
        return new Result(invalidOperation);
    }
}

public readonly struct Result<T>
{
    public Result(T result, InvalidOperation error = InvalidOperation.None)
    {
        Value = result;
        Error = error;
        IsNotApplicable = false;
    }

    public Result(InvalidOperation error)
    {
        Value = default;
        Error = error;
        IsNotApplicable = false;
    }

    private Result(InvalidOperation error, bool notApplicable)
    {
        Value = default;
        Error = error;
        IsNotApplicable = notApplicable;
    }

    public Result ResultValue => new(Error);

    public T Value { get; }
    public InvalidOperation Error { get; }
    public bool IsNotApplicable { get; }

    public bool Succeeded => Error is InvalidOperation.None;
    public bool Failed => Error is not InvalidOperation.None;

    public static Result<T> Success => new(InvalidOperation.None);

    public static Result<T> Ok(T value)
    {
        return new Result<T>(value);
    }


    public static Result<T> NotPossible => new(InvalidOperation.NotPossible);

    public static Result<T> Fail(InvalidOperation invalidOperation)
    {
        return new Result<T>(invalidOperation);
    }

    public static Result<T> NotApplicable => new(InvalidOperation.None, true);
}