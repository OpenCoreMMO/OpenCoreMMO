using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;
using System;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveItemContainerPacket(byte containerId, ushort slotIndex, IItem item) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerRemoveItem);

        message.AddByte(containerId);

        message.AddUInt16(slotIndex);

        //todo: 1098 implements this
        //if (lastItem)
        //{
        //    message.AddItem(lastItem);
        //}
        //else
        //{
        message.AddUInt16(0x00);
        //}
    }
}