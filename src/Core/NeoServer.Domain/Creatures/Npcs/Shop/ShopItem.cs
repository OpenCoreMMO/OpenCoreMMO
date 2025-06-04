using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Creatures.Npcs.Shop;

public record ShopItem(IItemType Item, uint BuyPrice, uint SellPrice, string CustomName = null) : IShopItem
{
}