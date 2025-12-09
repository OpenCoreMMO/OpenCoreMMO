using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.World;
using NeoServer.Domain.World.Map;
using NeoServer.Networking.Handlers;
using NeoServer.Scripts.LuaJIT.IoC;
using NeoServer.Server.Commands.Movements;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Commands.WaitingInLine;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Standalone.IoC.Modules;
using NeoServer.Server.Tasks;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Server.Standalone.IoC;

public static class Container
{
    private static readonly Lazy<Assembly[]> _assemblyCacheLazy = new(() =>
        AppDomain.CurrentDomain.GetAssemblies().AsParallel().Where(assembly =>
            !assembly.IsDynamic &&
            !assembly.FullName.StartsWith("System.") &&
            !assembly.FullName.StartsWith("Microsoft.") &&
            !assembly.FullName.StartsWith("Windows.") &&
            !assembly.FullName.StartsWith("mscorlib,") &&
            !assembly.FullName.StartsWith("Serilog,") &&
            !assembly.FullName.StartsWith("Autofac,") &&
            !assembly.FullName.StartsWith("netstandard,")).ToArray());
    
    internal static Assembly[] AssemblyCache => _assemblyCacheLazy.Value;

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

        //tools
        builder.AddSingleton<IPathFinder, PathFinder>();
        builder.AddSingleton<IWalkToMechanism, WalkToMechanism>();

        builder.RegisterPacketHandlers();

        builder.AddSingleton<IScheduler, OptimizedScheduler>();
        builder.AddSingleton<IDispatcher, Dispatcher>();
        builder.AddSingleton<IPersistenceDispatcher, PersistenceDispatcher>();

        //world
        builder.AddSingleton<IMap, Map>();
        builder.AddSingleton<World>();

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
            .AddCommands()
            .AddLua()
            .Register()
            .AddJobs()
            .AddRoutines()
            .AddDataStores();

        //creature
        builder.AddSingleton<ICreatureGameInstance, CreatureGameInstance>();

        //Waiting Queue Manager
        builder.AddSingleton<IWaitingQueueManager, WaitingQueueManager>();

        builder.AddSingleton(typeof(IMemoryCache), new MemoryCache(new MemoryCacheOptions()));

        return builder
            .BuildServiceProvider()
            .Verify(builder);
    }

    private static void RegisterPacketHandlers(this IServiceCollection builder)
    {
        var assemblies = Assembly.GetAssembly(typeof(PacketHandler));
        builder.RegisterAssemblyTypes(assemblies);
    }

    private static IServiceCollection AddCommands(this IServiceCollection builder)
    {
        var assembly = Assembly.GetAssembly(typeof(PlayerLogInCommand));
        builder.RegisterAssemblyTypes(assembly);
        return builder;
    }
}