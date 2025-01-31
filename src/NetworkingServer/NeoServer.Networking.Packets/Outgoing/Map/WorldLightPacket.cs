using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class WorldLightPacket(byte level, byte color) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.WorldLight;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(level);
        message.AddByte(color);
    }
}