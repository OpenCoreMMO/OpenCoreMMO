using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class MapSliceWestPacket(byte[] mapDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.MapSliceWest;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(mapDescription);
    }
}