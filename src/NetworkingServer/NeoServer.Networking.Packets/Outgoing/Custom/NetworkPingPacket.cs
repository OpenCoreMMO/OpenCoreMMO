using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class NetworkPingPacket : OutgoingPacket
{
    public required uint PingId { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.NetworkPing);
        message.AddUInt32(PingId);
    }
}