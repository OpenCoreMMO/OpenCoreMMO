using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.World.Models.Tiles;

public struct OperationResult : IOperationResult
{
    public List<(IThing, Operation, byte)> Operations { get; private set; }

    public void Add(Operation operation, IThing thing, byte stackPosition = 0)
    {
        Operations = Operations ?? new List<(IThing, Operation, byte)>();
        Operations.Add((thing, operation, stackPosition));
    }

    public OperationResult(Operation operation, IThing thing, byte stackPosition = 0)
    {
        Operations = new List<(IThing, Operation, byte)>();
        Operations.Add((thing, operation, stackPosition));
    }

    public bool HasAnyOperation => Operations?.Any() ?? false;
}