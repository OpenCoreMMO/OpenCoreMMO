using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Loaders.Interfaces;

namespace NeoServer.Loaders.Configurations;

public class ConfigurationLoader : IStartupLoader
{
    private readonly IPathFinder pathFinder;
    private readonly IWalkToMechanism walkToMechanism;

    public ConfigurationLoader(IPathFinder pathFinder, IWalkToMechanism walkToMechanism)
    {
        this.pathFinder = pathFinder;
        this.walkToMechanism = walkToMechanism;
    }

    public void Load()
    {
        GameToolStore.PathFinder = pathFinder;
        GameToolStore.WalkToMechanism = walkToMechanism;
    }
}