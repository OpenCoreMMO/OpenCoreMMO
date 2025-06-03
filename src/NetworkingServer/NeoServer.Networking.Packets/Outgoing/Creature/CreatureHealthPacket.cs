using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureHealthPacket : OutgoingPacket
{
    private readonly ICreature creature;

    public CreatureHealthPacket(ICreature creature)
    {
        this.creature = creature;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.CreatureHealth);

        message.AddUInt32(creature.CreatureId);

        if (creature.IsHealthHidden)
        {
            message.AddByte(0x00);
        }
        else
        {
            var result = (double)creature.HealthPoints / (int)Math.Max(creature.MaxHealthPoints, 1);
            result = Math.Ceiling(result * 100);

            message.AddByte((byte)result);
        }
    }
}