using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void ShowShop(INpc npc, ISociableCreature to, IEnumerable<IShopItem> shopItems);
public delegate void CloseShop(ISociableCreature to);

public interface IShopperNpc : INpc
{
    IDictionary<ushort, IShopItem> ShopItems { get; }

    event ShowShop OnShowShop;
    event CloseShop OnCloseShop;

    void StartSellingToCustomer(ISociableCreature creature);
    void StopSellingToCustomer(ISociableCreature creature);
    bool BuyFromCustomer(ISociableCreature creature, IItemType item, byte amount);
    ulong CalculateCost(IItemType itemType, byte amount);
}