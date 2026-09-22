using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NeoServer.Networking.Packets.Connection;
using NeoServer.Networking.Packets.Outgoing;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace NeoServer.Networking.Tests.Connections;

public class ConnectionSendExceptionTests
{
    [Theory]
    [InlineData(SocketError.ConnectionAborted)]
    [InlineData(SocketError.ConnectionReset)]
    [InlineData(SocketError.Shutdown)]
    [Trait("Category", "EdgeCase")]
    public void IsClientDisconnected_is_true_when_ioexception_wraps_socket_error(SocketError socketError)
    {
        var exception = new IOException(
            "Unable to write data to the transport connection.",
            new SocketException((int)socketError));

        Assert.True(Connection.IsClientDisconnected(exception));
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void IsClientDisconnected_is_true_when_ioexception_wraps_object_disposed()
    {
        var exception = new IOException(
            "Unable to write data to the transport connection.",
            new ObjectDisposedException(nameof(NetworkStream)));

        Assert.True(Connection.IsClientDisconnected(exception));
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void IsClientDisconnected_is_true_for_direct_disconnect_exceptions()
    {
        Assert.True(Connection.IsClientDisconnected(new SocketException((int)SocketError.ConnectionAborted)));
        Assert.True(Connection.IsClientDisconnected(new ObjectDisposedException("socket")));
        Assert.True(Connection.IsClientDisconnected(new EndOfStreamException("Connection closed by remote host")));
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public void IsClientDisconnected_is_false_when_send_failure_is_not_a_disconnect()
    {
        var exception = new IOException(
            "Unable to write data to the transport connection.",
            new InvalidOperationException("buffer rejected"));

        Assert.False(Connection.IsClientDisconnected(exception));
        Assert.False(Connection.IsClientDisconnected(new IOException("disk failure")));
        Assert.False(Connection.IsClientDisconnected(new InvalidOperationException("not a socket")));
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public async Task Connection_does_not_log_send_error_when_client_aborts()
    {
        var sink = new CollectingSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        TcpClient client = null;
        Connection connection = null;

        try
        {
            client = new TcpClient();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            await client.ConnectAsync(IPAddress.Loopback, port);

            var serverSocket = await listener.AcceptSocketAsync();
            connection = new Connection(serverSocket, logger);

            client.LingerState = new LingerOption(true, 0);
            client.Client.Close();

            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (DateTime.UtcNow < deadline && !sink.LoggedClientDisconnect)
            {
                connection.Send(new PingPacket());
                await Task.Delay(50);
            }

            Assert.False(sink.LoggedSendFailure, sink.AllText);
            Assert.False(sink.HasError, sink.AllText);
            Assert.True(sink.LoggedClientDisconnect, sink.AllText);
        }
        finally
        {
            connection?.Dispose();
            client?.Dispose();
            listener.Stop();
        }
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public async Task Connection_does_not_log_send_error_when_read_sees_client_abort()
    {
        var sink = new CollectingSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        TcpClient client = null;
        Connection connection = null;

        try
        {
            client = new TcpClient();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            await client.ConnectAsync(IPAddress.Loopback, port);

            var serverSocket = await listener.AcceptSocketAsync();
            connection = new Connection(serverSocket, logger);
            connection.BeginStreamRead();

            client.LingerState = new LingerOption(true, 0);
            client.Client.Close();

            await Task.Delay(200);
            connection.Send(new PingPacket());
            await Task.Delay(300);

            Assert.False(sink.LoggedSendFailure, sink.AllText);
            Assert.False(sink.HasError, sink.AllText);
        }
        finally
        {
            connection?.Dispose();
            client?.Dispose();
            listener.Stop();
        }
    }

    private sealed class CollectingSink : ILogEventSink
    {
        private readonly List<LogEvent> _events = [];
        private readonly object _gate = new();

        public bool HasError
        {
            get
            {
                lock (_gate)
                {
                    foreach (var logEvent in _events)
                    {
                        if (logEvent.Level >= LogEventLevel.Error)
                            return true;
                    }

                    return false;
                }
            }
        }

        public bool LoggedSendFailure
        {
            get
            {
                lock (_gate)
                {
                    foreach (var logEvent in _events)
                    {
                        if (logEvent.MessageTemplate.Text.Contains("Unable to send stream message", StringComparison.Ordinal))
                            return true;
                    }

                    return false;
                }
            }
        }

        public bool LoggedClientDisconnect
        {
            get
            {
                lock (_gate)
                {
                    foreach (var logEvent in _events)
                    {
                        var template = logEvent.MessageTemplate.Text;
                        if (template.Contains("Connection closed during send", StringComparison.Ordinal)
                            || template.Contains("Connection already closed", StringComparison.Ordinal)
                            || template.Contains("Socket already closed", StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        public string AllText
        {
            get
            {
                lock (_gate)
                {
                    if (_events.Count == 0)
                        return "(no log events)";

                    var lines = new List<string>(_events.Count);
                    foreach (var logEvent in _events)
                        lines.Add(logEvent.Level + ": " + logEvent.RenderMessage() + " " + logEvent.Exception);

                    return string.Join(Environment.NewLine, lines);
                }
            }
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_gate)
                _events.Add(logEvent);
        }
    }
}
