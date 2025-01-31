using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing;

public class PingPacket : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.Ping;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
    }
}