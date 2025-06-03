using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddAtStackPositionPacket : OutgoingPacket
{
    private readonly ICreature creature;
    private readonly byte stackPosition;

    public AddAtStackPositionPacket(ICreature creature, byte stackPosition)
    {
        this.creature = creature;
        this.stackPosition = stackPosition;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.AddAtStackPos);
        message.AddLocation(creature.Location);
        message.AddByte(stackPosition);
    }
}