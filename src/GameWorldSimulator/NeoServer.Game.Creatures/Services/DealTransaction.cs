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

public class DealTransaction : IDealTransaction
{
    private readonly ICoinTransaction _coinTransaction;
    private readonly ICoinTypeStore _coinTypeStore;
    private readonly IItemFactory _itemFactory;

    public DealTransaction(IItemFactory itemFactory, ICoinTransaction coinTransaction, ICoinTypeStore coinTypeStore)
    {
        _itemFactory = itemFactory;
        _coinTransaction = coinTransaction;
        _coinTypeStore = coinTypeStore;
    }

    public bool Buy(IPlayer buyer, IShopperNpc seller, IItemType itemType, byte amount)
    {
        if (buyer is null || seller is null || itemType is null || amount == 0) return false;

        var cost = seller.CalculateCost(itemType, amount);
        if (buyer.GetTotalMoney(_coinTypeStore) < cost) return false;

        var amountToAddToInventory = buyer.Inventory.CanAddItem(itemType).Value;
        var amountToAddToBackpack = buyer.Inventory.BackpackSlot?.CanAddItem(itemType).Value ?? 0;

        if (amount > amountToAddToBackpack + amountToAddToInventory) return false;

        var removedAmount = _coinTransaction.RemoveCoins(buyer, cost, out var change);

        if (removedAmount < cost) buyer.WithdrawFromBank(cost - removedAmount);

        var saleContract = new SaleContract(itemType.ServerId, amount, amountToAddToInventory, amountToAddToBackpack);

        AddItems(buyer, seller, saleContract);

        _coinTransaction.AddCoins(buyer, change);

        return true;
    }

    private void Sell(ISociableCreature part, ISociableCreature counterpart)
    {
    }

    private void AddItems(IPlayer player, INpc seller, SaleContract saleContract)
    {
        var item = _itemFactory.Create(saleContract.TypeId, Location.Inventory(Slot.Backpack), null);

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
                items[i] = _itemFactory.Create(saleContract.TypeId, Location.Inventory(Slot.Backpack), null);

            player.ReceivePurchasedItems(seller, saleContract, items);
        }
    }

    public IEnumerable<IItem> CreateCoins(ulong amount)
    {
        var coinsToAdd = CoinCalculator.Calculate(_coinTypeStore.Map, amount);

        foreach (var coinToAdd in coinsToAdd)
        {
            var createdCoin = _itemFactory.Create(coinToAdd.Item1, Location.Inventory(Slot.Backpack), null);
            if (createdCoin is not ICoin newCoin) continue;
            newCoin.Amount = coinToAdd.Item2;

            yield return newCoin;
        }
    }
}