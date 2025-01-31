using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureOutfitPacket(ICreature creature) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CreatureOutfit;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(creature.CreatureId);
        message.AddUInt16(creature.Outfit.LookType);

        if (creature.Outfit.LookType > 0)
        {
            message.AddByte(creature.Outfit.Head);
            message.AddByte(creature.Outfit.Body);
            message.AddByte(creature.Outfit.Legs);
            message.AddByte(creature.Outfit.Feet);
            message.AddByte(creature.Outfit.Addon);
        }
        else
        {
            message.AddUInt16(0); //todo
        }
    }
}