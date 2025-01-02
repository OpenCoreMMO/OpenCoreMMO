using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Creatures.Npcs.Shop;

public class ShopperNpc : Npc, IShopperNpc
{
    internal ShopperNpc(INpcType type, IMapTool mapTool, ISpawnPoint spawnPoint, IOutfit outfit = null,
        uint healthPoints = 0) : base(
        type, mapTool, spawnPoint, outfit, healthPoints)
    {
    }

    public Func<IDictionary<ushort, IItemType>> CoinTypeMapFunc { get; init; }

    public event ShowShop OnShowShop;

    public IDictionary<ushort, IShopItem> ShopItems
    {
        get
        {
            if (!Metadata.CustomAttributes.TryGetValue("shop", out var shop)) return null;

            if (shop is not IDictionary<ushort, IShopItem> shopItems) return null;

            return shopItems;
        }
    }

    public virtual void StopSellingToCustomer(ISociableCreature creature)
    {
        //todo: invoke event here
    }

    public bool BuyFromCustomer(ISociableCreature creature, IItemType item, byte amount)
    {
        var shopItems = ShopItems;
        if (shopItems is null) return false;

        if (!shopItems.TryGetValue(item.ServerId, out var shopItem)) return false;

        return Pay(creature, shopItem.SellPrice * amount);
    }

    public ulong CalculateCost(IItemType itemType, byte amount)
    {
        var shopItems = ShopItems;
        if (shopItems is null) return 0;

        if (!shopItems.TryGetValue(itemType.ServerId, out var shopItem)) return 0;

        return (shopItem?.BuyPrice ?? 0) * amount;
    }

    public bool Pay(ISociableCreature to, uint value)
    {
        if (to is not IPlayer player) return false;
        if (value == 0) return false;

        var coins = CoinCalculator.Calculate(CoinTypeMapFunc?.Invoke(), value);

        var items = new IItem[coins.Count()];

        var i = 0;
        foreach (var coin in coins)
        {
            var (coinType, amount) = coin;

            var item = CreateNewItem(coinType, Location.Inventory(Slot.Backpack),
                new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } });

            if (item is null) continue;
            items[i++] = item;
        }

        player.ReceivePayment(items, value);

        return true;
    }

    public override void SendMessageTo(ISociableCreature to, SpeechType type, IDialog dialog)
    {
        base.SendMessageTo(to, type, dialog);
        if (dialog.Action == "shop") ShowShopItems(to);
    }

    public virtual void ShowShopItems(ISociableCreature to)
    {
        if (to is not IPlayer player) return;

        if (ShopItems?.Values is not IEnumerable<IShopItem> shopItems) return;

        player.StartShopping(this);

        OnShowShop?.Invoke(this, to, shopItems);
    }
}