using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class MapSliceNorthPacket(byte[] mapDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.MapSliceNorth;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(mapDescription);
    }
}