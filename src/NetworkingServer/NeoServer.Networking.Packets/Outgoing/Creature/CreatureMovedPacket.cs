using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureMovedPacket(Location location, Location toLocation, byte stackPosition) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CreatureMoved;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddByte(stackPosition);
        message.AddLocation(toLocation);
    }
}