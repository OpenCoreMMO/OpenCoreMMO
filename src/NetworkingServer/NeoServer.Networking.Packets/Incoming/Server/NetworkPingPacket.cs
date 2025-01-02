using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming.Server;

public class NetworkPingPacket : IncomingPacket
{
    public NetworkPingPacket(IReadOnlyNetworkMessage message)
    {
        PingId = message.GetUInt32();
        LocalPing = message.GetUInt16();
        Fps = message.GetUInt16();
    }

    public uint PingId { get; }
    public ushort LocalPing { get; }
    public ushort Fps { get; }
}