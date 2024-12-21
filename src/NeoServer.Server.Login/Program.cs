using NeoServer.Data.Contexts;
using NeoServer.Game.Common;
using NeoServer.Loaders.Interfaces;
using NeoServer.Networking.Listeners;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Security;
using Serilog;
using System.Diagnostics;
using NeoServer.Server.Login.IoC;

Console.Title = "OpenCoreMMO Login Server";

var sw = new Stopwatch();
sw.Start();

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var container = Container.BuildConfigurations();

var (serverConfiguration, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
    container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

var (logger, _) = (container.Resolve<ILogger>(), container.Resolve<LoggerConfiguration>());

logger.Information("Welcome to OpenCoreMMO Login Server!");

logger.Information("Log set to: {Log}", logConfiguration.MinimumLevel);
logger.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ENVIRONMENT"));

container = Container.BuildAllLogin();
NeoServer.Server.Helpers.IoC.Initialize(container);

//GameAssemblyCache.Load();

await LoadDatabase(container, logger, cancellationToken);

Rsa.LoadPem(serverConfiguration.Data);

container.Resolve<IEnumerable<IRunBeforeLoaders>>().ToList().ForEach(x => x.Run());

container.Resolve<IEnumerable<IStartupLoader>>().ToList().ForEach(x => x.Load());

var scheduler = container.Resolve<IScheduler>();
var dispatcher = container.Resolve<IDispatcher>();
var persistenceDispatcher = container.Resolve<IPersistenceDispatcher>();

dispatcher.Start(cancellationToken);
scheduler.Start(cancellationToken);
persistenceDispatcher.Start(cancellationToken);

//container.Resolve<EventSubscriber>().AttachEvents();
container.Resolve<IEnumerable<IStartup>>().ToList().ForEach(x => x.Run());

StartListening(container, cancellationToken);

//container.Resolve<IGameServer>().Open();

sw.Stop();

logger.Step("Running Garbage Collector", "Garbage collected", () =>
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
});

logger.Information("Memory usage: {Mem} MB",
    Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1024f / 1024f, 2));

logger.Information("Login Server is {Up}! {Time} ms", "up", sw.ElapsedMilliseconds);

await Task.Delay(Timeout.Infinite, cancellationToken);

static async Task LoadDatabase(IServiceProvider container, ILogger logger,
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

static void StartListening(IServiceProvider container, CancellationToken cancellationToken)
{
    container.Resolve<LoginListener>().BeginListening(cancellationToken);
}