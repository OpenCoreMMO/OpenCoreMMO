using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class NetworkPingPacket(uint pingId) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.NetworkPing;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(pingId);
    }
}