using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Server.Common.Contracts.Network;
using System.Linq;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveItemContainerPacket(byte containerId, ushort slotIndex, IItem item) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerRemoveItem);

        message.AddByte(containerId);

        message.AddUInt16(slotIndex);

        //todo: 860/1098, parent cannot be null, if i need remove a item from container the container need to be the parent
        if(item.Parent is IContainer container)
        {
            var lastItem = container.Items.LastOrDefault();

            if (item == lastItem)
            {
                message.AddItem(lastItem);
            }
            else
            {
                message.AddUInt16(0x00);
            }
        }
        else
        {
            message.AddUInt16(0x00);
        }

        //todo: 1098 implements this
        //if (lastItem)
        //{
        //    message.AddItem(lastItem);
        //}
        //else
        //{
        //message.AddUInt16(0x00);
        //}
    }
}