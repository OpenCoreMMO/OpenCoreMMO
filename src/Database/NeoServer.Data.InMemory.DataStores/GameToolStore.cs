using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Data.InMemory.DataStores;

public class GameToolStore : DataStore<GameToolStore, string, object>
{
    public static IPathFinder PathFinder
    {
        get => Data.Get(nameof(PathFinder)) is not IPathFinder pathFinder ? null : pathFinder;
        set => Data.AddOrUpdate(nameof(PathFinder), value);
    }

    public static IWalkToMechanism WalkToMechanism
    {
        get => Data.Get(nameof(WalkToMechanism)) is not IWalkToMechanism walkToMechanism ? null : walkToMechanism;
        set => Data.AddOrUpdate(nameof(WalkToMechanism), value);
    }
}