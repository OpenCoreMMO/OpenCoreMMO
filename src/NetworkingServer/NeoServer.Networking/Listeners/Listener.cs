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

public abstract class Listener : TcpListener, IListener
{
    private readonly CancellationTokenSource _internalCancellation = new();
    private readonly ILogger _logger;
    private readonly int _port;
    private readonly IProtocol _protocol;

    private volatile bool _isShuttingDown;

    protected Listener(int port, IProtocol protocol, ILogger logger) : base(IPAddress.Any, port)
    {
        _port = port;
        _protocol = protocol;
        _logger = logger;
    }

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
                    _logger.Error(ex, "Could not start {Protocol} on port {Port} (attempt {Retry}/{MaxRetries})",
                        _protocol, _port, retryCount, maxRetries);

                    if (retryCount >= maxRetries)
                    {
                        _logger.Error("Failed to start {Protocol} after {MaxRetries} attempts. Giving up.", _protocol,
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

            _logger.Information("{Protocol} is online on port {Port}", _protocol, _port);

            try
            {
                while (!combinedCts.Token.IsCancellationRequested)
                {
                    var connection = await CreateConnectionAsync(combinedCts.Token);
                    if (connection != null) _protocol.OnAccept(connection);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Information("{Protocol} listener on port {Port} stopped", _protocol, _port);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error in {Protocol} listener on port {Port}", _protocol, _port);
            }
            finally
            {
                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Error stopping {Protocol} listener", _protocol);
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
            _logger.Warning(ex, "Error during listener shutdown");
        }
    }

    private async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var socket = await AcceptSocketAsync(cancellationToken).ConfigureAwait(false);

            var connection = new Connection(socket, _logger);

            connection.OnCloseEvent += OnConnectionClose;
            connection.OnProcessEvent += _protocol.ProcessMessage;
            connection.OnPostProcessEvent += _protocol.PostProcessMessage;

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
            _logger.Error(ex, "Error creating connection for {Protocol}", _protocol);
            return null;
        }
    }

    private void OnConnectionClose(object sender, IConnectionEventArgs args)
    {
        try
        {
            // De-subscribe to this event first.
            args.Connection.OnCloseEvent -= OnConnectionClose;
            args.Connection.OnProcessEvent -= _protocol.ProcessMessage;
            args.Connection.OnPostProcessEvent -= _protocol.PostProcessMessage;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error during connection cleanup");
        }
    }

    public void Dispose()
    {
        EndListening();
        _internalCancellation?.Dispose();
    }
}