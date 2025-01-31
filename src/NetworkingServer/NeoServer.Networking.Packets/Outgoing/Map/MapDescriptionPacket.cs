using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class MapDescriptionPacket(Location location, byte[] mapDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.MapDescription;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddBytes(mapDescription);
    }
}