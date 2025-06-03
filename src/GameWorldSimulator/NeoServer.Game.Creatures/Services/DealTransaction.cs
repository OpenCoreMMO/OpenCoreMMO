using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Creatures.Services;

public class DealTransaction(IItemFactory itemFactory, ICoinTransaction coinTransaction, ICoinTypeStore coinTypeStore) : IDealTransaction
{
    public bool PlayerBuyItem(IPlayer buyer, IShopperNpc seller, IItemType itemType, byte amount, bool ignoreCapacity = false, bool inBackpacks = false)
    {
        if (buyer is null || seller is null || itemType is null || amount == 0) return false;

        var cost = seller.CalculateCost(itemType, amount);
        if (buyer.GetTotalMoney(coinTypeStore) < cost) return false;

        var amountToAddToInventory = buyer.Inventory.CanAddItem(itemType).Value;
        var amountToAddToBackpack = buyer.Inventory.BackpackSlot?.CanAddItem(itemType).Value ?? 0;

        if (amount > amountToAddToBackpack + amountToAddToInventory) return false;

        var removedAmount = coinTransaction.RemoveCoins(buyer, cost, out var change);

        if (removedAmount < cost) buyer.WithdrawFromBank(cost - removedAmount);

        var saleContract = new SaleContract(itemType.ServerId, amount, amountToAddToInventory, amountToAddToBackpack);

        AddItems(buyer, seller, saleContract);

        coinTransaction.AddCoins(buyer, change);

        seller.OnPlayerBuyItem(buyer, itemType, amount, amountToAddToInventory, inBackpacks);

        return true;
    }

    public bool PlayerSellItem(IPlayer seller, IShopperNpc buyer, IItemType itemType, byte amount, bool ignoreEquipped = false)
    {
        if (!ignoreEquipped) return true;
        if (seller.Inventory.BackpackSlot?.Map is null) return false;
        if (!seller.Inventory.BackpackSlot.Map.TryGetValue(itemType.ServerId, out var itemTotalAmount)) return false;

        if (itemTotalAmount < amount) return false;

        seller.Inventory.BackpackSlot.RemoveItem(itemType, amount);

        var shopItems = buyer.ShopItems;
        if (shopItems is null) return false;

        if (!shopItems.TryGetValue(itemType.ServerId, out var shopItem)) return false;

        var totalCost = shopItem.SellPrice * amount;

        buyer.Pay(seller, totalCost);

        buyer.OnPlayerSellItem(seller, itemType, amount, totalCost, ignoreEquipped);

        return true;
    }

    private void AddItems(IPlayer player, INpc seller, SaleContract saleContract)
    {
        var item = itemFactory.Create(saleContract.TypeId, Location.Inventory(Slot.Backpack), null);

        if (item is ICumulative cumulative)
        {
            cumulative.Amount = saleContract.Amount;
            player.ReceivePurchasedItems(seller, saleContract, item);
        }
        else
        {
            var items = new IItem[saleContract.Amount];
            items[0] = item;

            for (var i = 1; i < saleContract.Amount; i++)
                items[i] = itemFactory.Create(saleContract.TypeId, Location.Inventory(Slot.Backpack), null);

            player.ReceivePurchasedItems(seller, saleContract, items);
        }
    }

    public IEnumerable<IItem> CreateCoins(ulong amount)
    {
        var coinsToAdd = CoinCalculator.Calculate(coinTypeStore.Map, amount);

        foreach (var coinToAdd in coinsToAdd)
        {
            var createdCoin = itemFactory.Create(coinToAdd.Item1, Location.Inventory(Slot.Backpack), null);
            if (createdCoin is not ICoin newCoin) continue;
            newCoin.Amount = coinToAdd.Item2;

            yield return newCoin;
        }
    }
}