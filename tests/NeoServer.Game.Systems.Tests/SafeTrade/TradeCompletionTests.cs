using FluentAssertions;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Item.Services;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Models.Tiles;
using NeoServer.Modules.Trading;
using NeoServer.Modules.Trading.TradeExchange;
using NeoServer.Modules.Trading.Validations;

namespace NeoServer.Game.Systems.Tests.SafeTrade;

public class TradeCompletionTests
{
    [Fact]
    public void Trade_completes_when_both_players_have_capacity()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(capacity: 100);
        var secondPlayer = PlayerTestDataBuilder.Build(capacity: 100);

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestData.CreateWeaponItem(1, weight: 100);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);

        player.Inventory.AddItem(item1, Slot.Left);
        secondPlayer.Inventory.AddItem(item2, Slot.Left);

        tradeSystem.Request(player, secondPlayer, item1);
        tradeSystem.Request(secondPlayer, player, item2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.None);

        player.Inventory[Slot.Left].Should().Be(item2);
        secondPlayer.Inventory[Slot.Left].Should().Be(item1);
    }

    [Fact]
    [ThreadBlocking]
    public void Trade_completes_when_both_players_have_free_slots()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(capacity: 1000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build(capacity: 1000);

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestData.CreateWeaponItem(1, weight: 100);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[100, 100, 7]).AddItem(item1);
        ((DynamicTile)map[101, 100, 7]).AddItem(item2);

        tradeSystem.Request(player, secondPlayer, item1);
        tradeSystem.Request(secondPlayer, player, item2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.None);

        ((DynamicTile)map[100, 100, 7]).TopItemOnStack.Should().NotBe(item1);
        ((DynamicTile)map[101, 100, 7]).TopItemOnStack.Should().NotBe(item2);

        player.Inventory.BackpackSlot.Items[0].Should().Be(item2);
        secondPlayer.Inventory.Weapon.Should().Be(item1);
    }

    [Fact]
    public void Trade_completes_when_both_players_trade_their_own_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var inventory2 = InventoryTestDataBuilder.GenerateInventory();

        var player = PlayerTestDataBuilder.Build(capacity: 1000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build(capacity: 1000, inventoryMap: inventory2);

        var backpackFromPlayer1 = player.Inventory.BackpackSlot;
        var backpackFromPlayer2 = secondPlayer.Inventory.BackpackSlot;

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);

        tradeSystem.Request(player, secondPlayer, backpackFromPlayer1);
        tradeSystem.Request(secondPlayer, player, backpackFromPlayer2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.None);

        player.Inventory.BackpackSlot.Should().Be(backpackFromPlayer2);
        secondPlayer.Inventory.BackpackSlot.Should().Be(backpackFromPlayer1);
    }

    [Fact]
    public void Trade_completes_when_both_players_have_free_slots_to_join_cumulative_item()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        inventory[Slot.Left] = (ItemTestData.CreateThrowableDistanceItem(1, 50, 1), 1);

        var player = PlayerTestDataBuilder.Build(capacity: 1000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build(capacity: 1000);

        player.Inventory.RemoveItem(Slot.Backpack, 1);

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestData.CreateThrowableDistanceItem(1, 50, 1);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[100, 100, 7]).AddItem(item1);
        ((DynamicTile)map[101, 100, 7]).AddItem(item2);

        tradeSystem.Request(player, secondPlayer, item1);
        tradeSystem.Request(secondPlayer, player, item2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.None);

        ((DynamicTile)map[100, 100, 7]).TopItemOnStack.Should().NotBe(item1);
        ((DynamicTile)map[101, 100, 7]).TopItemOnStack.Should().NotBe(item2);

        player.Inventory.Weapon.ServerId.Should().Be(1);
        player.Inventory.Weapon.Amount.Should().Be(100);

        secondPlayer.Inventory.Weapon.Should().Be(item1);
    }
}