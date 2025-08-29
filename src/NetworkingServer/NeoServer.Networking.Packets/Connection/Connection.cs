using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Networking.Packets.Security;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Packets.Connection;

public class Connection : IConnection
{
    private const uint NETWORK_MESSAGE_MAXSIZE = 24590u - 16u;
    private const int BUFFER_SIZE = 1024;
    private const byte HEADER_LENGTH = 2;

    private readonly ILogger _logger;
    private readonly Socket _socket;
    private readonly NetworkStream _stream;
    private readonly SemaphoreSlim _readSemaphore = new(1, 1);
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1);

    private volatile bool _isDisposed;
    private volatile bool _isReading;

    public Connection(Socket socket, ILogger logger)
    {
        _socket = socket;
        Ip = socket?.RemoteEndPoint?.ToString();
        _stream = new NetworkStream(_socket);
        XteaKey = new uint[4];
        IsAuthenticated = false;
        InMessage = new ReadOnlyNetworkMessage(new byte[16394], 0);
        _logger = logger;
        LastPingResponse = DateTime.UtcNow.Ticks;
        RandomNumber = (byte)new Random().Next(byte.MinValue, byte.MaxValue);
        TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private bool Closed => _isDisposed || !_stream.CanRead || !_socket.Connected;

    public Queue<IOutgoingPacket> OutgoingPackets { get; private set; }
    public IReadOnlyNetworkMessage InMessage { get; }
    public uint[] XteaKey { get; private set; }
    public uint CreatureId { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool Disconnected { get; private set; }
    public long LastPingRequest { get; set; }
    public long LastPingResponse { get; set; }
    public long TimeStamp { get; }
    public byte RandomNumber { get; }
    public ushort OtcV8Version { get; set; }
    public event EventHandler<IConnectionEventArgs> OnProcessEvent;
    public event EventHandler<IConnectionEventArgs> OnCloseEvent;
    public event EventHandler<IConnectionEventArgs> OnPostProcessEvent;
    public string Ip { get; }

    public void BeginStreamRead()
    {
        if (_isDisposed || _isReading) return;

        _ = Task.Run(async () =>
        {
            await ReadLoopAsync();
        });
    }

    private async Task ReadLoopAsync()
    {
        if (!await _readSemaphore.WaitAsync(100)) return;

        try
        {
            _isReading = true;

            while (!_isDisposed && _socket.Connected && !Disconnected)
            {
                try
                {
                    await ReadMessageAsync();
                }
                catch
                {
                    break;
                }
            }
        }
        finally
        {
            _isReading = false;
            _readSemaphore.Release();
        }
    }

    private async Task ReadMessageAsync()
    {
        // Read header first
        var headerBuffer = new byte[HEADER_LENGTH];
        await ReadExactAsync(headerBuffer, 0, HEADER_LENGTH);

        var messageSize = BitConverter.ToUInt16(headerBuffer, 0) + 2;
        
        if (messageSize >= NETWORK_MESSAGE_MAXSIZE)
        {
            Close(true);
            return;
        }

        // Copy header to main buffer
        Array.Copy(headerBuffer, InMessage.Buffer, HEADER_LENGTH);

        // Read remaining message if necessary
        if (messageSize > HEADER_LENGTH)
        {
            var remainingSize = Math.Min(messageSize - HEADER_LENGTH, BUFFER_SIZE - HEADER_LENGTH);
            await ReadExactAsync(InMessage.Buffer, HEADER_LENGTH, remainingSize);
        }

        InMessage.Resize(messageSize);

        // Process message in dispatcher to avoid callback hell
        var clientDisconnected = messageSize == 0;
        if (clientDisconnected && !IsAuthenticated)
        {
            Close();
            return;
        }

        if (clientDisconnected && IsAuthenticated) 
        {
            Disconnected = true;
        }

        var eventArgs = new ConnectionEventArgs(this);
        OnProcessEvent?.Invoke(this, eventArgs);
    }

    private async Task ReadExactAsync(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count && !_isDisposed)
        {
            int bytesRead = await _stream.ReadAsync(buffer, offset + totalRead, count - totalRead);
            if (bytesRead == 0)
                throw new EndOfStreamException("Connection closed by remote host");

            totalRead += bytesRead;
        }
    }

    public void SetXtea(uint[] xtea)
    {
        XteaKey = xtea;
    }

    public void Close(bool force = false)
    {
        if (_isDisposed) return;

        try
        {
            _isDisposed = true;

            if (!_socket.Connected)
            {
                _stream?.Close();
                return;
            }

            if (OutgoingPackets == null || OutgoingPackets.Count == 0 || force)
            {
                CloseSocket();
            }

            OnCloseEvent?.Invoke(this, new ConnectionEventArgs(this));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unable to close socket connection");
        }
    }

    public void SendFirstConnection()
    {
        var message = new NetworkMessage();

        new FirstConnectionPacket
        {
            RandomNumber = RandomNumber,
            TimeStamp = TimeStamp
        }.WriteToMessage(message);

        message.AddLength();

        _ = SendMessageAsync(message, false);
    }

    public void Send(IOutgoingPacket packet)
    {
        var message = new NetworkMessage();
        packet.WriteToMessage(message);
        message.AddLength();

        var encryptedMessage = Xtea.Encrypt(message, XteaKey);
        _logger.Debug("To {PlayerId}: {Name}", CreatureId, packet.GetType().Name);

        _ = SendMessageAsync(encryptedMessage);
    }

    public void Send()
    {
        if (OutgoingPackets.Count == 0) return;

        var message = new NetworkMessage();

        while (OutgoingPackets.TryDequeue(out var packet))
        {
            _logger.Debug("To {PlayerId}: {Name}", CreatureId, packet.GetType().Name);
            packet.WriteToMessage(message);
        }

        message.AddLength();
        var encryptedMessage = Xtea.Encrypt(message, XteaKey);
        _ = SendMessageAsync(encryptedMessage);
    }

    public void Disconnect(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            var message = new NetworkMessage();
            new LoginFailurePacket(text).WriteToMessage(message);
            message.AddLength();
            var encryptedMessage = Xtea.Encrypt(message, XteaKey);
            _ = SendMessageAsync(encryptedMessage);
        }

        Close();
    }

    public void SetAsAuthenticated()
    {
        IsAuthenticated = true;
    }

    public void SetConnectionOwner(IPlayer player)
    {
        if (CreatureId != 0) throw new InvalidOperationException("Connection already has a Player Id");
        SetAsAuthenticated();
        OutgoingPackets = new Queue<IOutgoingPacket>();
        CreatureId = player.CreatureId;
    }

    private async Task SendMessageAsync(INetworkMessage message, bool addHeader = true)
    {
        if (!await _writeSemaphore.WaitAsync(1000))
        {
            _logger.Warning("Send timeout on connection {IP}", Ip);
            return;
        }

        try
        {
            if (Closed || !_socket.Connected || Disconnected) return;

            var streamMessage = addHeader ? message.AddHeader() : message.GetMessageInBytes().ToArray();
            await _stream.WriteAsync(streamMessage, 0, streamMessage.Length);
            await _stream.FlushAsync();

            var eventArgs = new ConnectionEventArgs(this);
            OnPostProcessEvent?.Invoke(this, eventArgs);
        }
        catch (Exception ex) when (ex is ObjectDisposedException or SocketException)
        {
            _logger.Debug("Connection closed during send: {Exception}", ex.Message);
            Close();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unable to send stream message");
            Close();
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }

    private void CloseSocket()
    {
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Unable to close socket gracefully");
        }
    }

    public void Dispose()
    {
        Close(true);
        _readSemaphore?.Dispose();
        _writeSemaphore?.Dispose();
        _stream?.Dispose();
    }
}