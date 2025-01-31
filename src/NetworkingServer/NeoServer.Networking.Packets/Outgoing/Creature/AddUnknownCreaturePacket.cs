using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class AddUnknownCreaturePacket(IPlayer player, IWalkableCreature creatureToAdd) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.AddUnknownCreature;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(creatureToAdd.GetRaw(player, false));
    }
}