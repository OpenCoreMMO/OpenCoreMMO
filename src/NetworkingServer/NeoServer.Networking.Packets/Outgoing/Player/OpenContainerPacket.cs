using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Server.Common.Contracts.Network;
using System;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class OpenContainerPacket : OutgoingPacket
{
    private readonly IContainer container;
    private readonly byte containerId;

    public OpenContainerPacket(IContainer container, byte containerId)
    {
        this.container = container;
        this.containerId = containerId;
    }

    public required bool WithDescription { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerOpen);

        message.AddByte(containerId);

        const int ITEM_BROWSEFIELD = 460;

        if (container.ServerId == ITEM_BROWSEFIELD)
        {
            //todo: 1098 implement this
            //message.AddItem(ITEM_BAG, 1);
            //message.AddString("Browse Field");
        }
        else
        {
            message.AddItem(container, WithDescription);
            message.AddString(container.Name);
        }

        message.AddByte(container.Capacity);

        message.AddByte(container.HasParent ? (byte)0x01 : (byte)0x00);

        message.AddByte(0x01); //container.IsUnlocked ? 0x01 : 0x00); // Drag and drop //todo: 1098 impelment this
        message.AddByte(0x00); // container->hasPagination() ? 0x01 : 0x00); // Pagination //todo: 1098 implement this

        message.AddUInt16((ushort)container.Items.Count);//todo: 1098 implement this //containerSize
        message.AddUInt16(0);//todo: 1098 implement this //firstIndex

        if (container.HasItems)
        {
            message.AddByte((byte)container.Items.Count);
            for (byte i = 0; i < container.SlotsUsed; i++)
                message.AddItem(container.Items[i], WithDescription);
        }
        else {
            message.AddByte(0x00);
        }

        //todo: 1098 implement this
        //if (firstIndex < containerSize)
        //{
        //    uint8_t itemsToSend = std::min<uint32_t>(std::min<uint32_t>(container->capacity(), containerSize - firstIndex), std::numeric_limits < uint8_t >::max());

        //    msg.addByte(itemsToSend);
        //    for (auto it = container->getItemList().begin() + firstIndex, end = it + itemsToSend; it != end; ++it)
        //    {
        //        msg.addItem(*it);
        //    }
        //}
        //else
        //{
        //    msg.addByte(0x00);
        //}
    }
}