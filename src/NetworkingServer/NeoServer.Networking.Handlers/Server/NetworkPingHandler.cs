using NeoServer.Networking.Packets.Incoming.Server;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Server;

public class NetworkPingHandler: PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var packet = new NetworkPingPacket(message);
        
        connection.Send(new Packets.Outgoing.Custom.NetworkPingPacket()
        {
            PingId = packet.PingId
        });
    }
}