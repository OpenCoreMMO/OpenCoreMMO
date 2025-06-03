using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IDealTransaction
{
    bool PlayerBuyItem(IPlayer buyer, IShopperNpc seller, IItemType itemType, byte amount, bool ignoreCapacity = false, bool inBackpacks = false);
    bool PlayerSellItem(IPlayer seller, IShopperNpc buyer, IItemType itemType, byte amount, bool ignoreEquipped = false);
}