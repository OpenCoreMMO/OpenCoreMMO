using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Domain;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Dispatcher;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Creature;
using NeoServer.Game.World;
using NeoServer.Game.World.Map;
using NeoServer.Modules.Chat;
using NeoServer.Modules.Combat.PlayerAttack.RuneAttack;
using NeoServer.Modules.Creatures;
using NeoServer.PacketHandler.Features.Shared;
using NeoServer.Server.Standalone.IoC.Modules;
using NeoServer.Shared.IoC.Modules;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Server.Standalone.IoC;

public static class Container
{
    internal static Assembly[] AssemblyCache => AppDomain.CurrentDomain.GetAssemblies().AsParallel().Where(assembly =>
        !assembly.IsDynamic &&
        !assembly.FullName.StartsWith("System.") &&
        !assembly.FullName.StartsWith("Microsoft.") &&
        !assembly.FullName.StartsWith("Windows.") &&
        !assembly.FullName.StartsWith("mscorlib,") &&
        !assembly.FullName.StartsWith("Serilog,") &&
        !assembly.FullName.StartsWith("Autofac,") &&
        !assembly.FullName.StartsWith("Mediator.") &&
        !assembly.FullName.StartsWith("netstandard,")).ToArray();

    public static IServiceProvider BuildConfigurations()
    {
        var builder = new ServiceCollection();

        var configuration = ConfigurationInjection.GetConfiguration();

        builder
            .AddConfigurations(configuration)
            .AddLogger(configuration);

        return builder
            .BuildServiceProvider()
            .Verify(builder);
    }

    public static IServiceProvider BuildAll()
    {
        var builder = new ServiceCollection();

        builder.AddMediator();

        builder.RegisterAssembliesByInterface(typeof(ISingleton));

        //tools
        builder.AddSingleton<IPathFinder, PathFinder>()
            .AddSingleton<IWalkToMechanism, WalkToMechanism>()

            //thread management
            .AddSingleton<IScheduler, OptimizedScheduler>()
            .AddSingleton<IDispatcher, Dispatcher>()
            .AddSingleton<IPersistenceDispatcher, PersistenceDispatcher>();

        //world
        builder.AddSingleton<IMap, Map>();
        builder.AddSingleton<World>();

        builder.AddSingleton<AttackRuneCooldownManager>();
        builder
            .AddSingleton<NeoServer.BuildingBlocks.Application.Contracts.IGameCreatureManager, GameCreatureManager>();
        builder.AddSingleton<IWalkToTarget, WalkToTarget>();
        builder.AddSingleton<IChatModule, ChatModule>();
        builder.AddSingleton<IEventBus, InMemoryEventBus>();

        builder.RegisterAssembliesByInterface(typeof(IIntegrationEventHandler<>));

        var configuration = ConfigurationInjection.GetConfiguration();

        builder.AddFactories()
            .AddServices()
            .AddLoaders()
            .AddDatabases(configuration)
            .AddRepositories()
            .AddConfigurations(configuration)
            .AddNetwork()
            .AddEvents()
            .AddManagers()
            .AddLogger(configuration)
            .AddLua()
            .AddJobs()
            .AddDataStores()
            .AddFeatures()
            .RegisterModules()
            .RegisterPacketHandlers();
        
        //creature
        builder.AddSingleton<ICreatureGameInstance, CreatureGameInstance>();

        builder.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        return builder
            .BuildServiceProvider()
            .Verify(builder);
    }

    private static IServiceCollection RegisterPacketHandlers(this IServiceCollection builder)
    {
        _ = Assembly.GetAssembly(typeof(PacketHandler.PacketHandler));
        return builder.RegisterAssembliesByInterface(typeof(PacketHandler.IPacketHandler));
    }
}