using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Application.Extensions;
using NeoServer.BuildingBlocks.Application.Server;
using NeoServer.BuildingBlocks.Application.Server.Jobs.Persistence;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Dispatcher;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Extensions.Compiler;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Infrastructure.Database.Contexts;
using NeoServer.Loaders.Interfaces;
using NeoServer.Loaders.Items;
using NeoServer.Loaders.Monsters;
using NeoServer.Loaders.Quest;
using NeoServer.Loaders.Spawns;
using NeoServer.Loaders.Spells;
using NeoServer.Loaders.Vocations;
using NeoServer.Loaders.World;
using NeoServer.Modules.Chat.Channel;
using NeoServer.Modules.Creatures;
using NeoServer.Modules.Creatures.Routines;
using NeoServer.Modules.ItemManagement.DecayManagement;
using NeoServer.Networking.Listeners;
using NeoServer.Networking.Packets.Security;
using NeoServer.PacketHandler;
using NeoServer.Scripts.Lua;
using NeoServer.Server.Standalone.IoC;
using Serilog;

namespace NeoServer.Server.Standalone;

public class Program
{
    public static async Task Main()
    {  
        Console.Title = "OpenCoreMMO Server";

        var sw = new Stopwatch();
        sw.Start();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var container = Container.BuildConfigurations();

        var (serverConfiguration, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
            container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

        var (logger, _) = (container.Resolve<ILogger>(), container.Resolve<LoggerConfiguration>());

        logger.Information("Welcome to OpenCoreMMO Server!");

        logger.Information("Log set to: {Log}", logConfiguration.MinimumLevel);
        logger.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "N/A");

        logger.Step("Building extensions...", "{files} extensions build",
            () => ExtensionsCompiler.Compile(serverConfiguration.Data, serverConfiguration.Extensions));

        container = Container.BuildAll();
        BuildingBlocks.Application.IoC.Initialize(container);

        GameAssemblyCache.Load();

        await LoadDatabase(container, logger, cancellationToken);

        Rsa.LoadPem(serverConfiguration.Data);

        container.Resolve<EventSubscriber>().AttachEvents();

        container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());
        container.Resolve<CreatureFactoryEventSubscriber>().AttachEvents();

        container.Resolve<ItemTypeLoader>().Load();

        container.Resolve<QuestDataLoader>().Load();

        container.Resolve<WorldLoader>().Load();

        container.Resolve<SpawnLoader>().Load();

        container.Resolve<MonsterLoader>().Load();
        container.Resolve<VocationLoader>().Load();
        container.Resolve<SpellLoader>().Load();

        container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());

        container.Resolve<SpawnManager>().StartSpawn();

        var scheduler = container.Resolve<IScheduler>();
        var dispatcher = container.Resolve<IDispatcher>();
        var persistenceDispatcher = container.Resolve<IPersistenceDispatcher>();

        dispatcher.Start(cancellationToken);
        scheduler.Start(cancellationToken);
        persistenceDispatcher.Start(cancellationToken);

        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameCreatureJob>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<DecayRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameChatChannelRoutine>().StartChecking));
        container.Resolve<PlayerPersistenceJob>().Start(cancellationToken);

        container.Resolve<IEnumerable<IStartup>>().ToList().ForEach(x => x.Run());

        container.Resolve<LuaGlobalRegister>().Register();
        container.Resolve<IEventBus>().Initialize();

        StartListening(container, cancellationToken);

        container.Resolve<IGameServer>().Open();

        sw.Stop();

        logger.Step("Running Garbage Collector", "Garbage collected", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        });

        logger.Information("Memory usage: {Mem} MB",
            Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1024f / 1024f, 2));

        logger.Information("Server is {Up}! {Time} ms", "up", sw.ElapsedMilliseconds);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private static async Task LoadDatabase(IServiceProvider container, ILogger logger,
        CancellationToken cancellationToken)
    {
        var (_, databaseName) = container.Resolve<DatabaseConfiguration>();
        var context = container.Resolve<NeoContext>();

        logger.Information("Loading database: {Db}", databaseName);

        try
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to connect to database");
            Environment.Exit(0);
        }

        logger.Information("{Db} database loaded", databaseName);
    }

    private static void StartListening(IServiceProvider container, CancellationToken cancellationToken)
    {
        container.Resolve<LoginListener>().BeginListening(cancellationToken);
        container.Resolve<GameListener>().BeginListening(cancellationToken);
    }
}