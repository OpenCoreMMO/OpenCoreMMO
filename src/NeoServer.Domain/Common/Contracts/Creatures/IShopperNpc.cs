using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void ShowShop(INpc npc, ISociableCreature to, IEnumerable<IShopItem> shopItems);

public delegate void CloseShop(ISociableCreature to);

public delegate void BuyItem(INpc npc, IPlayer player, IItemType itemType, int count, uint totalCost, bool inBackpack,
    bool ignore = false);

public delegate void SellItem(INpc npc, IPlayer player, IItemType itemType, int count, uint totalCost,
    bool ignore = false);

public interface IShopperNpc : INpc
{
    IDictionary<ushort, IShopItem> ShopItems { get; }

    event ShowShop OnShowShop;
    event CloseShop OnCloseShop;

    event SellItem OnSellItem;
    event BuyItem OnBuyItem;

    void StartSellingToCustomer(ISociableCreature creature, IEnumerable<IShopItem> shopItems = null);
    void StopSellingToCustomer(ISociableCreature creature);
    ulong CalculateCost(IItemType itemType, byte amount);
    bool Pay(IPlayer player, uint value);

    public void OnPlayerBuyItem(IPlayer player, IItemType itemType, int count, uint totalCost, bool inBackpack,
        bool ignore = false);

    public void OnPlayerSellItem(IPlayer player, IItemType itemType, int count, uint totalCost, bool ignore = false);
}