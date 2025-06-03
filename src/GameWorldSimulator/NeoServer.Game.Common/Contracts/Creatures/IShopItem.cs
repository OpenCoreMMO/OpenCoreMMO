using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IShopItem
{
    IItemType Item { get; }
    uint BuyPrice { get; }
    uint SellPrice { get; }
    string CustomName { get; }
}