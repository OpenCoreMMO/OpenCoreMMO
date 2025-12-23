using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Configurations;
using NeoServer.Server.Events.Subscribers;
using NeoServer.Server.Helpers;
using NeoServer.Server.Routines.Channels;
using NeoServer.Server.Routines.Creatures;
using NeoServer.Server.Routines.Items;
using NeoServer.Server.Routines.Persistence;
using NeoServer.Server.Routines.World;
using NeoServer.Server.Standalone.IoC;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Server.Tests;

public class TestSetup
{
    public static async Task<IServiceProvider> Setup()
    {
        var container = BuildContainer();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        GameAssemblyCache.Load();

        var (serverConfiguration, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
            container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

        // Preload OTBM to speed up world loading
        var otbmLoadTask = WorldLoader.PreLoadOtbm(serverConfiguration, cancellationToken);

        var context = container.GetService<NeoContext>();
        var command = container.GetService<PlayerLogInCommand>();
        var game = container.GetService<IGameServer>();
        var logger = container.GetService<ILogger>();

        container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());
        container.Resolve<FactoryEventSubscriber>().AttachEvents();

        await LoadDatabase(container, logger, new CancellationToken(false));

        container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());
        container.Resolve<FactoryEventSubscriber>().AttachEvents();

        container.Resolve<ItemTypeLoader>().Load();
        container.Resolve<QuestDataLoader>().Load();

        container.Resolve<VocationLoader>().Load();
        container.Resolve<SpellLoader>().Load();

        container.Resolve<MonsterLoader>().Load();
        container.Resolve<GroupLoader>().Load();

        container.Resolve<WorldLoader>().Load(await otbmLoadTask);
        container.Resolve<SpawnLoader>().Load();

        container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());

        container.Resolve<IScriptManager>().Initialize();

        container.Resolve<SpawnManager>().StartSpawn();

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

        container.Resolve<IEventAggregator>().Initialize();

        return container;
    }

    private static IServiceProvider BuildContainer()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Test");

        var container = Container.BuildConfigurations();

        container = Container.BuildAll();
        IoC.Initialize(container);
        return container;
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
}