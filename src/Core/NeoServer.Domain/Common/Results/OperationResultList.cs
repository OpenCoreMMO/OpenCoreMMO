namespace NeoServer.Domain.Common.Results;

public struct OperationResultList<T>
{
    public List<(T, Operation, byte)> Operations { get; private set; }

    public void Add(Operation operation, T thing, byte position = 0)
    {
        Operations ??= new List<(T, Operation, byte)>();
        Operations.Add((thing, operation, position));
    }

    public OperationResultList(Operation operation, T thing, byte position = 0)
    {
        Operations = new List<(T, Operation, byte)> { (thing, operation, position) };
    }

    public OperationResultList(T value)
    {
        Operations = new List<(T, Operation, byte)> { (value, Operation.None, 0) };
    }

    public bool HasAnyOperation => Operations?.Any() ?? false;
}