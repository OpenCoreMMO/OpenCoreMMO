using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Npc;

public class SaleItemListPacket(IPlayer player, IEnumerable<IShopItem> shopItems, ICoinTypeStore coinTypeStore) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.SaleItemList;

    public override void WriteToMessage(INetworkMessage message)
    {
        if (player is null || shopItems is null) return;

        var map = player.Inventory.Map;
        var totalMoney = player.Inventory.GetTotalMoney(coinTypeStore) + player.BankAmount;

        message.AddByte(PacketType);
        message.AddUInt32((uint)Math.Min(totalMoney, uint.MaxValue));

        byte itemsToSend = 0;

        var temp = new List<byte>();
        foreach (var shopItem in shopItems)
        {
            if (shopItem.SellPrice == 0) continue;

            var index = (int)shopItem.Item.ServerId;
            //if (Item::items[shopInfo.itemId].isFluidContainer()) //todo
            //{
            //    index |= (static_cast<uint32_t>(shopInfo.subType) << 16);
            //}

            if (!map.TryGetValue(shopItem.Item.ServerId, out var itemAmount)) continue;

            temp.AddRange(BitConverter.GetBytes(shopItem.Item.ClientId));
            temp.Add((byte)Math.Min(itemAmount, byte.MaxValue));

            if (++itemsToSend >= byte.MaxValue) break;
        }

        message.AddByte(itemsToSend);
        message.AddBytes(temp.ToArray());
    }
}