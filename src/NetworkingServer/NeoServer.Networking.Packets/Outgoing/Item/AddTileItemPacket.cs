using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddTileItemPacket : OutgoingPacket
{
    private readonly IItem item;
    private readonly byte stackPosition;

    public AddTileItemPacket(IItem item, byte stackPosition)
    {
        this.item = item;
        this.stackPosition = stackPosition;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.AddAtStackPos);
        message.AddLocation(item.Location);
        message.AddByte(stackPosition);
        message.AddItem(item);
    }
}