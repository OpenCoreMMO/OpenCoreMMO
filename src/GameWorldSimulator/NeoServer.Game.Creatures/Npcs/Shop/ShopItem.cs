using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Creatures.Npcs.Shop;

public record ShopItem(IItemType Item, uint BuyPrice, uint SellPrice, string CustomName = null) : IShopItem
{
}