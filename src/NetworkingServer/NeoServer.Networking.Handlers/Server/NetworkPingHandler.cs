using NeoServer.Networking.Packets.Incoming.Server;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Server;

public class NetworkPingHandler : PacketHandler
{
    private readonly IDispatcher _dispatcher;

    public NetworkPingHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var packet = new NetworkPingPacket(message);
        _dispatcher.AddEvent(new Event(()=> connection.Send(new Packets.Outgoing.Custom.NetworkPingPacket(packet.PingId))));
    }
}