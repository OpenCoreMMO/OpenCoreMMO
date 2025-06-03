using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IShopItem
{
    IItemType Item { get; }
    uint BuyPrice { get; }
    uint SellPrice { get; }
    string CustomName { get; }
}