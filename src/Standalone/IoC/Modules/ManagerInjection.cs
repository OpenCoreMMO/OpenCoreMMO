using Microsoft.Extensions.DependencyInjection;
using NeoServer.Domain.Depot;
using NeoServer.Domain.Spells;
using NeoServer.Domain.World.Models.Spawns;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Managers;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class ManagerInjection
{
    public static IServiceCollection AddManagers(this IServiceCollection builder)
    {
        builder.AddSingleton<IGameServer, GameServer>();
        builder.AddSingleton<IGameCreatureManager, GameCreatureManager>();
        builder.AddSingleton<IDecayableItemManager, DecayableItemManager>();


        builder.AddSingleton<SpawnManager>();
        builder.AddSingleton<DepotManager>();
        builder.AddSingleton<SpellListManager>();
        return builder;
    }
}