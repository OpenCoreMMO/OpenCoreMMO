using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.World.Tiles;

public interface IOperationResult
{
    List<(IThing, Operation, byte)> Operations { get; }
    bool HasAnyOperation { get; }

    void Add(Operation operation, IThing thing, byte stackPosition = 0);
}