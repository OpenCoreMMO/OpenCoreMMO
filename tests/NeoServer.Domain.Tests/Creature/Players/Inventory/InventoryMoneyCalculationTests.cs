using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players.Inventory;

public class InventoryMoneyCalculationTests
{
    [Fact]
    public void Inventory_total_money_is_the_sum_of_coins_in_backpack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var backpack = ItemTestDataBuilder.CreateBackpack(5);

        inventory.AddItem(backpack);

        var platinum = ItemTestDataBuilder.CreateCoin(1, 50, 100);
        var gold = ItemTestDataBuilder.CreateCoin(2, 10, 1);
        var crystal = ItemTestDataBuilder.CreateCoin(3, 2, 10_000);

        var bag = ItemTestDataBuilder.CreateBackpack(5);

        bag.AddItem(crystal);
        backpack.AddItem(platinum);
        backpack.AddItem(gold);
        backpack.AddItem(bag);

        ICoinTypeStore coinTypeStore = new CoinTypeStore();
        coinTypeStore.AddOrUpdate(1, platinum.Metadata);
        coinTypeStore.AddOrUpdate(2, gold.Metadata);
        coinTypeStore.AddOrUpdate(3, crystal.Metadata);

        //assert
        inventory.GetTotalMoney(coinTypeStore).Should().Be(25010);
    }
}