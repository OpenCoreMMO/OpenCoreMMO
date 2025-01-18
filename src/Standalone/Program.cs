using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Data.Contexts;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Aggregator;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Loaders.Groups;
using NeoServer.Loaders.Interfaces;
using NeoServer.Loaders.Items;
using NeoServer.Loaders.Monsters;
using NeoServer.Loaders.Quest;
using NeoServer.Loaders.Spawns;
using NeoServer.Loaders.Spells;
using NeoServer.Loaders.Vocations;
using NeoServer.Loaders.World;
using NeoServer.Networking.Listeners;
using NeoServer.Scripts.Lua;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Compiler;
using NeoServer.Server.Configurations;
using NeoServer.Server.Events.Subscribers;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Routines.Channels;
using NeoServer.Server.Routines.Creatures;
using NeoServer.Server.Routines.Items;
using NeoServer.Server.Routines.Persistence;
using NeoServer.Server.Security;
using NeoServer.Server.Standalone.IoC;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Server.Standalone;

public class Program
{
    private static CancellationTokenSource _cancellationTokenSource;
    private static CancellationToken _cancellationToken;

    public static async Task Main()
    {
        Console.Title = "OpenCoreMMO Server";

        var sw = new Stopwatch();
        sw.Start();

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        var container = Container.BuildConfigurations();

        var (serverConfiguration, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
            container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

        var (logger, _) = (container.Resolve<ILogger>(), container.Resolve<LoggerConfiguration>());

        logger.Information("Welcome to OpenCoreMMO Server!");

        logger.Information("Log set to: {Log}", logConfiguration.MinimumLevel);
        logger.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ENVIRONMENT"));

        logger.Step("Building extensions...", "{files} extensions build",
            () => ExtensionsCompiler.Compile(serverConfiguration.Data, serverConfiguration.Extensions));

        container = Container.BuildAll();
        Helpers.IoC.Initialize(container);

        GameAssemblyCache.Load();

        await LoadDatabase(container, logger, _cancellationToken);

        Rsa.LoadPem(serverConfiguration.Data);

        container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());
        container.Resolve<FactoryEventSubscriber>().AttachEvents();

        container.Resolve<ItemTypeLoader>().Load();

        container.Resolve<QuestDataLoader>().Load();

        container.Resolve<WorldLoader>().Load();

        container.Resolve<SpawnLoader>().Load();

        container.Resolve<MonsterLoader>().Load();
        container.Resolve<VocationLoader>().Load();
        container.Resolve<SpellLoader>().Load();
        container.Resolve<GroupLoader>().Load();

        container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());

        container.Resolve<SpawnManager>().StartSpawn();

        var scheduler = container.Resolve<IScheduler>();
        var dispatcher = container.Resolve<IDispatcher>();
        var persistenceDispatcher = container.Resolve<IPersistenceDispatcher>();

        dispatcher.Start(_cancellationToken);
        scheduler.Start(_cancellationToken);
        persistenceDispatcher.Start(_cancellationToken);

        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameCreatureRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameItemRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameChatChannelRoutine>().StartChecking));
        container.Resolve<PlayerPersistenceRoutine>().Start(_cancellationToken);

        container.Resolve<EventSubscriber>().AttachEvents();
        container.Resolve<IEnumerable<IStartup>>().ToList().ForEach(x => x.Run());

        container.Resolve<LuaGlobalRegister>().Register();
        container.Resolve<IScriptGameManager>().Start();
        EventAggregator.Instance.Initialize(container);

        StartListening(container, _cancellationToken);

        container.Resolve<IGameServer>().Open();

        sw.Stop();

        logger.Step("Running Garbage Collector", "Garbage collected", () =>
        {
            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
        });

        logger.Information("Memory usage: {Mem} MB",
            Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1024f / 1024f, 2));

        logger.Information("Server is {Up}! {Time} ms", "up", sw.ElapsedMilliseconds);

        SetupShutdownHandlers(logger, container);

        try { await Task.Delay(Timeout.Infinite, _cancellationToken); }
        catch (TaskCanceledException) { }
        catch (Exception ex) { logger.Error(ex, "Unhandled exception occurred."); }
        finally { await Shutdown(logger, container); }
    }

    private static void SetupShutdownHandlers(ILogger logger, IServiceProvider container)
    {
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _cancellationTokenSource.Cancel();
            eventArgs.Cancel = true;
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            Shutdown(logger, container).Wait();
            _cancellationTokenSource.Cancel();
        };
    }
    private static async Task Shutdown(ILogger logger, IServiceProvider container)
    {
        logger.Warning("Server is in Shutdown...");
        container.Resolve<IScriptGameManager>().GlobalEventExecuteShutdown();
        await container.Resolve<PlayerPersistenceRoutine>().SavePlayers();
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