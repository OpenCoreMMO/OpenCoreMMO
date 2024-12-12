using FluentAssertions;
using NeoServer.BuildingBlocks.Infrastructure.Data.InMemory;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Tests.Creature.Players.Inventory;

public class InventoryMoneyCalculationTests
{
    [Fact]
    public void Inventory_total_money_is_the_sum_of_coins_in_backpack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var backpack = ItemTestData.CreateBackpack(5);

        inventory.AddItem(backpack);

        var platinum = ItemTestData.CreateCoin(1, 50, 100);
        var gold = ItemTestData.CreateCoin(2, 10, 1);
        var crystal = ItemTestData.CreateCoin(3, 2, 10_000);

        var bag = ItemTestData.CreateBackpack(5);

        bag.AddItem(crystal);
        backpack.AddItem(platinum);
        backpack.AddItem(gold);
        backpack.AddItem(bag);

        ICoinTypeStore coinTypeStore = new CoinTypeStore();
        coinTypeStore.Add(1, platinum.Metadata);
        coinTypeStore.Add(2, gold.Metadata);
        coinTypeStore.Add(3, crystal.Metadata);

        //assert
        inventory.GetTotalMoney(coinTypeStore).Should().Be(25010);
    }
}