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
        message.AddByte((byte)GameOutgoingPacketType.TransformThing);

        if (stackPosition >= 10)
        {
            message.AddUInt16(0xFFFF);
            message.AddUInt32(creature.CreatureId);
        }
        else
        {
            message.AddLocation(creature.Location);
            message.AddByte(stackPosition);
        }

        message.AddUInt16((byte)GameOutgoingPacketType.CreatureTurn);
        message.AddUInt32(creature.CreatureId);
        message.AddByte((byte)direction);
        message.AddByte(0x00); //todo: 1098 msg.addByte(player->canWalkthroughEx(creature) ? 0x00 : 0x01);
    }
}