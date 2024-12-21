// See https://aka.ms/new-console-template for more information
using NeoServer.Data.Contexts;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common;
using NeoServer.Game.World.Models.Spawns;
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
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Events.Subscribers;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Routines.Channels;
using NeoServer.Server.Routines.Creatures;
using NeoServer.Server.Routines.Items;
using NeoServer.Server.Routines.Persistence;
using NeoServer.Server.Standalone.IoC;
using NeoServer.Server.Tasks;
using Serilog;
using System.Diagnostics;
using NeoServer.Server.Compiler;
using NeoServer.Server.Security;

Console.Title = "OpenCoreMMO Game Server";

var sw = new Stopwatch();
sw.Start();

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var container = Container.BuildConfigurations();

var (serverConfiguration, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
    container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

var (logger, _) = (container.Resolve<ILogger>(), container.Resolve<LoggerConfiguration>());

logger.Information("Welcome to OpenCoreMMO Game Server!");

logger.Information("Log set to: {Log}", logConfiguration.MinimumLevel);
logger.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ENVIRONMENT"));

logger.Step("Building extensions...", "{files} extensions build",
    () => ExtensionsCompiler.Compile(serverConfiguration.Data, serverConfiguration.Extensions));

container = Container.BuildAll();
NeoServer.Server.Helpers.IoC.Initialize(container);

GameAssemblyCache.Load();

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

container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());

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
container.Resolve<PlayerPersistenceRoutine>().Start(cancellationToken);

container.Resolve<EventSubscriber>().AttachEvents();
container.Resolve<IEnumerable<IStartup>>().ToList().ForEach(x => x.Run());

container.Resolve<LuaGlobalRegister>().Register();

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

static void StartListening(IServiceProvider container, CancellationToken cancellationToken)
{
    container.Resolve<GameListener>().BeginListening(cancellationToken);
}