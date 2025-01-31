using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class TransformThingPacket(ICreature creature, byte stackPosition) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.TransformThing;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(creature.Location);
        message.AddByte(stackPosition);
    }
}