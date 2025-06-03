namespace NeoServer.Domain.Common.Results;

public readonly struct OperationResult<T>
{
    public OperationResult(T result, InvalidOperation error = InvalidOperation.None,
        Operation operation = Operation.None)
    {
        Value = result;
        Error = error;
        Operation = operation;
    }

    public OperationResult(InvalidOperation error)
    {
        Value = default;
        Error = error;
        Operation = Operation.None;
    }

    public Operation Operation { get; }
    public T Value { get; }
    public InvalidOperation Error { get; }
    public bool Succeeded => Error is InvalidOperation.None;
    public bool Failed => Error is not InvalidOperation.None;

    public static OperationResult<T> Success => new(InvalidOperation.None);

    public static OperationResult<T> Ok(T value, Operation operation)
    {
        return new OperationResult<T>(value, operation: operation);
    }

    public static OperationResult<T> NotPossible => new(InvalidOperation.NotPossible);

    public static OperationResult<T> Fail(InvalidOperation invalidOperation)
    {
        return new OperationResult<T>(invalidOperation);
    }
}