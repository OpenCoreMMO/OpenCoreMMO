using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddTileItemPacket(IItem item, byte stackPosition) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.AddAtStackPos);
        message.AddLocation(item.Location);
        message.AddByte(stackPosition);
        message.AddItem(item);
    }
}