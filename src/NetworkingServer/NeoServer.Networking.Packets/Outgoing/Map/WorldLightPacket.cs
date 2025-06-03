using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class WorldLightPacket : OutgoingPacket
{
    private readonly byte Color;
    private readonly byte Level;

    public WorldLightPacket(byte level, byte color = 0xD7)
    {
        Level = level;
        Color = color;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.WorldLight);

        message.AddByte(Level);
        message.AddByte(Color);
    }
}