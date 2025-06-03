using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Npc;

public class OpenShopPacket(IEnumerable<IShopItem> items) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.OpenShop);

        var itemsCount = (ushort)Math.Min(items.Count(), ushort.MaxValue);
        message.AddByte((byte)itemsCount);

        foreach (var item in items) SendShopItem(message, item);
    }

    private void SendShopItem(INetworkMessage message, IShopItem shopItem)
    {
        if (shopItem is null) return;

        message.AddUInt16(shopItem.Item.ClientId);

        //if (it.isSplash() || it.isFluidContainer())
        //{
        //    msg.addByte(serverFluidToClient(item.subType));
        //}
        //else //todo
        {
            message.AddByte(0x00);
        }

        message.AddString(string.IsNullOrEmpty(shopItem.CustomName) ? shopItem.Item.Name : shopItem.CustomName);
        message.AddUInt32((uint)shopItem.Item.Weight * 100);
        message.AddUInt32(shopItem.BuyPrice);
        message.AddUInt32(shopItem.SellPrice);
    }
}