using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Domain.Common;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Standalone.Hosting;
using NeoServer.Server.Standalone.IoC;
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

        var (_, _, logConfiguration) = (container.Resolve<ServerConfiguration>(),
            container.Resolve<GameConfiguration>(), container.Resolve<LogConfiguration>());

        var (logger, _) = (container.Resolve<ILogger>(), container.Resolve<LogConfiguration>());

        logger.Information("Welcome to OpenCoreMMO Server [Baiak Version]!");
        logger.Information("Log set to: {Log}", logConfiguration.MinimumLevel);
        logger.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ENVIRONMENT"));

        var runtime = await ServerBootstrap.StartAsync(_cancellationToken);

        sw.Stop();

        logger.Step("Running Garbage Collector", "Garbage collected", () =>
        {
            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
        });

        logger.Information("Memory usage: {Mem} MB",
            Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1024f / 1024f, 2));

        logger.Information("Server is {Up}! {Time} ms", "up", sw.ElapsedMilliseconds);

        SetupShutdownHandlers(logger, runtime.Services);

        try
        {
            await Task.Delay(Timeout.Infinite, _cancellationToken);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unhandled exception occurred");
        }
        finally
        {
            await ServerBootstrap.ShutdownAsync(runtime.Services);
        }
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
            {
                return;
            }

            ServerBootstrap.ShutdownAsync(container).Wait();
            _cancellationTokenSource.Cancel();
        };
    }
}
