using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Creatures;
using NeoServer.Game.World;
using NeoServer.Game.World.Map;
using NeoServer.Networking.Handlers;
using NeoServer.Server.Commands.Movements;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Login.IoC.Modules;
using NeoServer.Server.Tasks;
using NeoServer.Server.Login.IoC.Modules;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Server.Login.IoC;

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
        !assembly.FullName.StartsWith("netstandard,")).ToArray();

    public static IServiceProvider BuildConfigurations()
    {
        var builder = new ServiceCollection();

        var configuration = ConfigurationInjection.GetConfiguration();

        builder
            .AddConfigurations(configuration)
            .AddLogger(configuration);

        return builder
            .BuildServiceProvider();
            //.Verify(builder);
    }

    public static IServiceProvider BuildAllLogin()
    {
        var builder = new ServiceCollection();

        //tools
        builder.RegisterPacketHandlers();

        builder.AddSingleton<IScheduler, OptimizedScheduler>();
        builder.AddSingleton<IDispatcher, Dispatcher>();
        builder.AddSingleton<IPersistenceDispatcher, PersistenceDispatcher>();

        //world
       
        var configuration = ConfigurationInjection.GetConfiguration();

        builder.AddFactoriesLogin()
            //.AddServices()
            //.AddLoaders()
            .AddDatabases(configuration)
            .AddRepositories()
            .AddConfigurations(configuration)
            .AddNetworkLogin()
            //.AddEvents()
            //.AddManagers()
            .AddLogger(configuration);
            //.AddCommands()
            //.AddLua()
            //.AddJobs()
            //.AddDataStores();

        //creature
        builder.AddSingleton(typeof(IMemoryCache), new MemoryCache(new MemoryCacheOptions()));

        return builder
            .BuildServiceProvider();
            //.Verify(builder);
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