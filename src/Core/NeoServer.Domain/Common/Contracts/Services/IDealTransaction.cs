using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IDealTransaction
{
    bool PlayerBuyItem(IPlayer buyer, IShopperNpc seller, IItemType itemType, byte amount, bool ignoreCapacity = false,
        bool inBackpacks = false);

    bool PlayerSellItem(IPlayer seller, IShopperNpc buyer, IItemType itemType, byte amount,
        bool ignoreEquipped = false);
}