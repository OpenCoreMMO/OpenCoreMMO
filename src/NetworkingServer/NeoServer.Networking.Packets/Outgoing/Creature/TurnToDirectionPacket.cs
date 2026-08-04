using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class TurnToDirectionPacket : OutgoingPacket
{
    private readonly ICreature creature;
    private readonly Direction direction;
    private readonly byte stackPosition;

    public TurnToDirectionPacket(ICreature creature, Direction direction, byte stackPosition)
    {
        this.creature = creature;
        this.direction = direction;
        this.stackPosition = stackPosition;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        if (direction is Direction.None) return;

        message.AddByte((byte)GameOutgoingPacketType.TransformThing);
        message.AddLocation(creature.Location);
        message.AddByte(stackPosition);
        message.AddUInt16((byte)GameOutgoingPacketType.CreatureTurn);
        message.AddUInt32(creature.CreatureId);
        message.AddByte((byte)direction);
    }
}