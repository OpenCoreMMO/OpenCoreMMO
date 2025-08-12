using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddItemContainerPacket(byte containerId, ushort slotIndex, IItem item) : OutgoingPacket
{
    public required bool ShowItemDescription { get; init; }


    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerAddItem);

        message.AddByte(containerId);
        message.AddUInt16(slotIndex);
        message.AddItem(item, ShowItemDescription);
    }
}