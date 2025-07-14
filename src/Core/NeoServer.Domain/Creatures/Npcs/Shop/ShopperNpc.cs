using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Domain.Creatures.Npcs.Shop;

public class ShopperNpc : Npc, IShopperNpc
{
    internal ShopperNpc(INpcType type, IMapTool mapTool, ISpawnPoint spawnPoint, IOutfit outfit = null,
        uint healthPoints = 0) : base(
        type, mapTool, spawnPoint, outfit, healthPoints)
    {
    }

    public Func<IDictionary<ushort, IItemType>> CoinTypeMapFunc { get; init; }

    public event ShowShop OnShowShop;
    public event CloseShop OnCloseShop;

    public IDictionary<ushort, IShopItem> ShopItems
    {
        get
        {
            if (Metadata.ShopItems.Count > 0)
                return Metadata.ShopItems;

            if (!Metadata.CustomAttributes.TryGetValue("shop", out var shop)) return null;

            if (shop is not IDictionary<ushort, IShopItem> shopItems) return null;

            return shopItems;
        }
    }


    public virtual void StopSellingToCustomer(ISociableCreature creature)
    {
        OnCloseShop?.Invoke(creature);
    }

    public virtual void StartSellingToCustomer(ISociableCreature creature, IEnumerable<IShopItem> shopItems)
    {
        ShowShopItems(creature, shopItems);
    }

    public ulong CalculateCost(IItemType itemType, byte amount)
    {
        var shopItems = ShopItems;
        if (shopItems is null) return 0;

        if (!shopItems.TryGetValue(itemType.ServerId, out var shopItem)) return 0;

        return (shopItem?.BuyPrice ?? 0) * amount;
    }

    public bool Pay(IPlayer player, uint value)
    {
        if (value == 0) return false;

        var coins = CoinCalculator.Calculate(CoinTypeMapFunc?.Invoke(), value);

        var items = new IItem[coins.Count()];

        var i = 0;
        foreach (var coin in coins)
        {
            var (coinType, amount) = coin;

            var item = CreateNewItem(coinType, Location.Inventory(Slot.Backpack),
                new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } }, null);

            if (item is null) continue;
            items[i++] = item;
        }

        player.ReceivePayment(items, value);

        return true;
    }


    public void OnPlayerBuyItem(IPlayer player, IItemType itemType, int count, uint totalCost, bool inBackpack,
        bool ignore = false)
    {
        OnBuyItem?.Invoke(this, player, itemType, count, totalCost, inBackpack, ignore);
    }

    public void OnPlayerSellItem(IPlayer player, IItemType itemType, int count, uint totalCost, bool ignore = false)
    {
        OnSellItem?.Invoke(this, player, itemType, count, totalCost, ignore);
    }

    public virtual void ShowShopItems(ISociableCreature to, IEnumerable<IShopItem> shopItems = null)
    {
        if (to is not IPlayer player) return;

        if (ShopItems?.Values is IEnumerable<IShopItem>)
            shopItems = shopItems ?? ShopItems.Values;

        if (shopItems != null && !shopItems.Any()) return;

        player.StartShopping(this);

        OnShowShop?.Invoke(this, to, shopItems);
    }

    #region Events

    public event SellItem OnSellItem;
    public event BuyItem OnBuyItem;

    #endregion
}