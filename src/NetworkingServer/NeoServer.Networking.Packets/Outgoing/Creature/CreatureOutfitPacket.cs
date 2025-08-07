using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureOutfitPacket : OutgoingPacket
{
    private readonly ICreature creature;

    public CreatureOutfitPacket(ICreature creature)
    {
        this.creature = creature;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.CreatureOutfit);

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
            // For items/creatures with LookType 0, use LookTypeEx (item ID)
            message.AddUInt16(0); // lookTypeEx - needs to be implemented if using items
        }
        
        // Add mount (required by TFS protocol)
        message.AddUInt16(0); // lookMount - mount ID, 0 if no mount
    }
}