using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddAtStackPositionPacket(ICreature creature, byte stackPosition) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.AddAtStackPos;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(creature.Location);
        message.AddByte(stackPosition);
    }
}