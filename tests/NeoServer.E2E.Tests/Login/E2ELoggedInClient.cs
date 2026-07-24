using NeoServer.E2E.Tests.Client;
using NeoServer.E2E.Tests.Client.Protocol;
using NeoServer.Networking.Packets.Messages;

namespace NeoServer.E2E.Tests.Login;

public sealed class E2ELoggedInClient(ProtocolTestClient client, uint[] xteaKey) : IAsyncDisposable
{
    public ProtocolTestClient Client { get; } = client;

    public uint[] XteaKey { get; } = xteaKey;

    public async Task SendGamePacketAsync(NetworkMessage message, CancellationToken cancellationToken = default)
    {
        var packet = ProtocolFraming.BuildEncryptedGamePacket(message, XteaKey);
        await Client.SendAsync(packet, cancellationToken);
    }

    public Task<byte[]> ReceivePacketAsync(CancellationToken cancellationToken = default)
    {
        return Client.ReceiveEncryptedPacketAsync(decryptAtIndexSix: false, cancellationToken);
    }

    public async Task DrainPacketsAsync(
        TimeSpan idleTimeout,
        CancellationToken cancellationToken = default)
    {
        var idleDeadline = DateTime.UtcNow + idleTimeout;

        while (DateTime.UtcNow < idleDeadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Client.HasPendingData())
            {
                await Task.Delay(25, cancellationToken);
                continue;
            }

            await ReceivePacketAsync(cancellationToken);
            idleDeadline = DateTime.UtcNow + idleTimeout;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync();
    }
}
