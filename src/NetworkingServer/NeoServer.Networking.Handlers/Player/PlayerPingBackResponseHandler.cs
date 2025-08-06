using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Player;

public sealed class PlayerPingBackResponseHandler : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        connection.Send(new PingBackPacket());
    }
}