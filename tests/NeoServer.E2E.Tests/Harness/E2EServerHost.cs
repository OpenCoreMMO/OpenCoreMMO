using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Networking.Listeners;
using NeoServer.Server.Configurations;
using NeoServer.Server.Standalone.Hosting;
using NeoServer.Server.Standalone.IoC;

namespace NeoServer.E2E.Tests.Harness;

public sealed class E2EServerHost : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ServerBootstrap.ServerRuntime _runtime;

    private E2EServerHost(
        ServerBootstrap.ServerRuntime runtime,
        int loginPort,
        int gamePort,
        CancellationTokenSource cancellationTokenSource)
    {
        _runtime = runtime;
        Services = runtime.Services;
        LoginPort = loginPort;
        GamePort = gamePort;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public IServiceProvider Services { get; }

    public int LoginPort { get; }

    public int GamePort { get; }

    public static async Task<E2EServerHost> StartAsync()
    {
        var (loginPort, gamePort) = AllocatePorts();
        Environment.SetEnvironmentVariable("ENVIRONMENT", "E2E");
        Environment.SetEnvironmentVariable("SERVER_LOGIN_PORT", loginPort.ToString());
        Environment.SetEnvironmentVariable("SERVER_GAME_PORT", gamePort.ToString());

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var runtime = await ServerBootstrap.StartAsync(cancellationToken, compileExtensions: false);

        await E2ETestDataSeeder.SeedAsync(runtime.Services, cancellationToken);

        var configuration = runtime.Services.Resolve<ServerConfiguration>();
        if (configuration.ServerLoginPort != loginPort || configuration.ServerGamePort != gamePort)
        {
            throw new InvalidOperationException(
                $"E2E server ports mismatch. Expected login {loginPort} and game {gamePort}, " +
                $"but got login {configuration.ServerLoginPort} and game {configuration.ServerGamePort}.");
        }

        await WaitForListenersAsync(loginPort, gamePort, cancellationToken);

        return new E2EServerHost(runtime, loginPort, gamePort, cancellationTokenSource);
    }

    public async ValueTask DisposeAsync()
    {
        var loginListener = Services.Resolve<LoginListener>();
        var gameListener = Services.Resolve<GameListener>();

        loginListener.EndListening();
        gameListener.EndListening();

        await _cancellationTokenSource.CancelAsync();
        await ServerBootstrap.ShutdownAsync(_runtime.Services);

        _cancellationTokenSource.Dispose();
    }

    private static (int LoginPort, int GamePort) AllocatePorts()
    {
        var loginPort = AllocatePort();
        var gamePort = AllocatePort();

        while (gamePort == loginPort)
        {
            gamePort = AllocatePort();
        }

        return (loginPort, gamePort);
    }

    private static int AllocatePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task WaitForListenersAsync(int loginPort, int gamePort, CancellationToken cancellationToken)
    {
        const int maxAttempts = 50;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await CanConnectAsync("127.0.0.1", loginPort, cancellationToken) &&
                await CanConnectAsync("127.0.0.1", gamePort, cancellationToken))
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new InvalidOperationException("E2E server listeners did not become ready in time.");
    }

    private static async Task<bool> CanConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cancellationToken);
            return client.Connected;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
