using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Npc;

public class OpenShopPacket(IEnumerable<IShopItem> items) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.OpenShop;

    public override void WriteToMessage(INetworkMessage message)
    {
        var itemsCount = (ushort)Math.Min(items.Count(), ushort.MaxValue);

        message.AddByte(PacketType);
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

        message.AddString(shopItem.Item.Name);
        message.AddUInt32((uint)shopItem.Item.Weight * 100);
        message.AddUInt32(shopItem.BuyPrice);
        message.AddUInt32(shopItem.SellPrice);
    }
}