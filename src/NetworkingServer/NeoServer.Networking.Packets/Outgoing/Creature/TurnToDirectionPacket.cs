using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class TurnToDirectionPacket(ICreature creature, Direction direction) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CreatureTurn;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16((byte)GameOutgoingPacketType.CreatureTurn);
        message.AddUInt32(creature.CreatureId);
        message.AddByte((byte)direction);
    }
}