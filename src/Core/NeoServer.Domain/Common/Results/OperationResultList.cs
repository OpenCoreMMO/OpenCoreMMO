namespace NeoServer.Domain.Common.Results;

public class OperationResultList<T>
{
    public OperationResultList()
    {
        Operations = null;
    }

    public OperationResultList(Operation operation, T thing, byte position = 0)
    {
        Operations = [(thing, operation, position)];
    }

    public OperationResultList(T value)
    {
        Operations = [(value, Operation.None, 0)];
    }

    public List<(T, Operation, byte)> Operations { get; private set; }

    public bool HasAnyOperation => Operations is { Count: > 0 };

    public void Add(Operation operation, T thing, byte position = 0)
    {
        Operations ??= [];
        Operations.Add((thing, operation, position));
    }
}