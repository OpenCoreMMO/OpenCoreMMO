using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Networking.Packets.Connection;
using NeoServer.Networking.Protocols;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Listeners;

public abstract class Listener(int port, IProtocol protocol, ILogger logger)
    : TcpListener(IPAddress.Any, port), IListener
{
    private readonly CancellationTokenSource _internalCancellation = new();
    private readonly int _port = port;

    private volatile bool _isShuttingDown;

    public void BeginListening(CancellationToken cancellationToken)
    {
        var combinedCts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalCancellation.Token);

        Task.Factory.StartNew(async () =>
        {
            var retryCount = 0;
            const int maxRetries = 5;

            while (!combinedCts.Token.IsCancellationRequested && retryCount < maxRetries)
                try
                {
                    Start();
                    retryCount = 0; // Reset retry count on successful start
                    break;
                }
                catch (SocketException ex)
                {
                    retryCount++;
                    logger.Error(ex, "Could not start {Protocol} on port {Port} (attempt {Retry}/{MaxRetries})",
                        protocol, _port, retryCount, maxRetries);

                    if (retryCount >= maxRetries)
                    {
                        logger.Error("Failed to start {Protocol} after {MaxRetries} attempts. Giving up.", protocol,
                            maxRetries);
                        return;
                    }

                    try
                    {
                        await Task.Delay(5000 * retryCount, combinedCts.Token); // Exponential backoff
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }

            logger.Information("{Protocol} is online on port {Port}", protocol, _port);

            try
            {
                while (!combinedCts.Token.IsCancellationRequested)
                {
                    var connection = await CreateConnectionAsync(combinedCts.Token);
                    if (connection != null) protocol.OnAccept(connection);
                }
            }
            catch (OperationCanceledException)
            {
                logger.Information("{Protocol} listener on port {Port} stopped", protocol, _port);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error in {Protocol} listener on port {Port}", protocol, _port);
            }
            finally
            {
                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, "Error stopping {Protocol} listener", protocol);
                }
            }
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void EndListening()
    {
        if (_isShuttingDown) return;

        _isShuttingDown = true;
        _internalCancellation.Cancel();

        try
        {
            Stop();
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error during listener shutdown");
        }
    }

    private async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var socket = await AcceptSocketAsync(cancellationToken).ConfigureAwait(false);

            var connection = new Connection(socket, logger);

            connection.OnCloseEvent += OnConnectionClose;
            connection.OnProcessEvent += protocol.ProcessMessage;
            connection.OnPostProcessEvent += protocol.PostProcessMessage;

            return connection;
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            return null;
        }
        catch (ObjectDisposedException)
        {
            // Expected during shutdown
            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating connection for {Protocol}", protocol);
            return null;
        }
    }

    private void OnConnectionClose(object sender, IConnectionEventArgs args)
    {
        try
        {
            // De-subscribe to this event first.
            args.Connection.OnCloseEvent -= OnConnectionClose;
            args.Connection.OnProcessEvent -= protocol.ProcessMessage;
            args.Connection.OnPostProcessEvent -= protocol.PostProcessMessage;
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error during connection cleanup");
        }
    }

    public void Dispose()
    {
        EndListening();
        _internalCancellation?.Dispose();
    }
}