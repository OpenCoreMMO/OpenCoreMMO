using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class AddKnownCreaturePacket(IPlayer player, IWalkableCreature creatureToAdd) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.AddKnownCreature;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(creatureToAdd.GetRaw(player, true));
    }
}