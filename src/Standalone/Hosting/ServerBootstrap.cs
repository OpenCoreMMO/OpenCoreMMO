using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Data.Contexts;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.World;
using NeoServer.Domain.World.Models.Spawns;
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
using NeoServer.Server.Routines.World;
using NeoServer.Server.Security;
using NeoServer.Server.Standalone.IoC;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Server.Standalone.Hosting;

public static class ServerBootstrap
{
    public sealed record ServerRuntime(IServiceProvider Services);

    public static async Task<ServerRuntime> StartAsync(
        CancellationToken cancellationToken,
        bool compileExtensions = true)
    {
        var container = Container.BuildConfigurations();

        var (serverConfiguration, _, _) = (container.Resolve<ServerConfiguration>(),
            container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

        var otbmLoadTask = WorldLoader.PreLoadOtbm(serverConfiguration, cancellationToken);
        var logger = container.Resolve<ILogger>();

        if (compileExtensions)
        {
            logger.Step("Building extensions...", "{files} extensions build",
                () => ExtensionsCompiler.Compile(serverConfiguration.Data, serverConfiguration.Extensions));
        }

        container = Container.BuildAll();
        Helpers.IoC.Initialize(container);

        GameAssemblyCache.Load();

        await LoadDatabase(container, logger, cancellationToken);

        Rsa.LoadPem(serverConfiguration.Data);

        container.Resolve<IEventAggregator>().Initialize();

        container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());
        container.Resolve<FactoryEventSubscriber>().AttachEvents();

        container.Resolve<ItemTypeLoader>().Load();
        container.Resolve<QuestDataLoader>().Load();
        container.Resolve<VocationLoader>().Load();
        container.Resolve<SpellLoader>().Load();
        container.Resolve<GroupLoader>().Load();
        container.Resolve<MonsterLoader>().Load();
        container.Resolve<WorldLoader>().Load(await otbmLoadTask);
        container.Resolve<SpawnLoader>().Load();
        container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());
        container.Resolve<IScriptManager>().Initialize();

        var scheduler = container.Resolve<IScheduler>();
        var dispatcher = container.Resolve<IDispatcher>();
        var persistenceDispatcher = container.Resolve<IPersistenceDispatcher>();

        dispatcher.Start(cancellationToken);
        scheduler.Start(cancellationToken);
        persistenceDispatcher.Start(cancellationToken);

        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameCreatureRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameItemRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameChatChannelRoutine>().StartChecking));
        scheduler.AddEvent(new SchedulerEvent(WorldLight.EVENT_WORLD_LIGHT_INTERVAL,
            container.Resolve<GameWorldRoutine>().StartChecking));

        container.Resolve<PlayerPersistenceRoutine>().Start(cancellationToken);
        container.Resolve<EventSubscriber>().AttachEvents();
        container.Resolve<IEnumerable<IStartup>>().ToList().ForEach(x => x.Run());
        container.Resolve<SpawnManager>().StartSpawn();

        StartListening(container, cancellationToken);
        container.Resolve<IGameServer>().Open();

        return new ServerRuntime(container);
    }

    public static async Task ShutdownAsync(IServiceProvider container)
    {
        var logger = container.Resolve<ILogger>();

        logger.Warning("Server is in Shutdown...");

        container.Resolve<IScriptManager>().GlobalEvents.ExecuteShutdown();
        await container.Resolve<PlayerPersistenceRoutine>().SavePlayers();

        container.Resolve<LoginListener>().Dispose();
        container.Resolve<GameListener>().Dispose();

        await container.Resolve<IDispatcher>().WaitForCompletionAsync();
        container.Resolve<IDispatcher>().Dispose();

        await container.Resolve<IPersistenceDispatcher>().WaitForCompletionAsync();
        container.Resolve<IPersistenceDispatcher>().Dispose();
    }

    private static async Task LoadDatabase(IServiceProvider container, ILogger logger,
        CancellationToken cancellationToken)
    {
        var (_, databaseName, dropOnStartup) = container.Resolve<DatabaseConfiguration>();
        var context = container.Resolve<NeoContext>();

        logger.Information("Loading database: {Db}", databaseName);

        try
        {
            if (dropOnStartup)
            {
                await context.Database.EnsureDeletedAsync(cancellationToken);
            }

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
