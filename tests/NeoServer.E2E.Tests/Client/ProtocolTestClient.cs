using System.Net.Sockets;
using NeoServer.E2E.Tests.Client.Protocol;

namespace NeoServer.E2E.Tests.Client;

public sealed class ProtocolTestClient(string host, int port) : IAsyncDisposable
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private uint[] _xteaKey;

    public bool IsConnected => _tcpClient?.Connected ?? false;

    public bool HasPendingData() => _stream?.DataAvailable ?? false;

    public IReadOnlyList<ReceivedPacket> ReceivedPackets => _receivedPackets;

    private readonly List<ReceivedPacket> _receivedPackets = [];

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return;
        }

        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port, cancellationToken);
        _stream = _tcpClient.GetStream();
    }

    public async Task SendAsync(byte[] payload, CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        await _stream.WriteAsync(payload, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    public async Task<byte[]> ReceiveLengthPrefixedPacketAsync(CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        return await ProtocolFraming.ReadLengthPrefixedPacketAsync(_stream, cancellationToken);
    }

    public async Task<byte[]> ReceiveEncryptedPacketAsync(
        bool decryptAtIndexSix,
        CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        var payload = await ProtocolFraming.ReadServerPacketAsync(
            _stream,
            _xteaKey,
            decryptAtIndexSix,
            cancellationToken);

        if (payload.Length >= 3)
        {
            var opcode = ProtocolFraming.ReadOpcode(payload);
            RecordReceivedPacket(opcode, payload);
        }

        return payload;
    }

    public void SetXteaKey(uint[] xteaKey)
    {
        _xteaKey = xteaKey;
    }

    public void RecordReceivedPacket(byte opcode, byte[] payload)
    {
        _receivedPackets.Add(new ReceivedPacket(opcode, payload));
    }

    public async ValueTask DisposeAsync()
    {
        if (_stream is not null)
        {
            await _stream.DisposeAsync();
            _stream = null;
        }

        _tcpClient?.Dispose();
        _tcpClient = null;
    }
}
