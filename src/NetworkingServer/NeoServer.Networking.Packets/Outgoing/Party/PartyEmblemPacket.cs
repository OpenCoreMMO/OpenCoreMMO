using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Party;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Party;

public class PartyEmblemPacket(ICreature creature, PartyEmblem emblem) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CreatureEmblem;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(creature.CreatureId);
        message.AddByte((byte)emblem);
    }
}