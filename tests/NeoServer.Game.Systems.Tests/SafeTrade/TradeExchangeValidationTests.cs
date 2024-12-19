using System.Linq;
using FluentAssertions;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Items.Services;
using NeoServer.Game.Systems.SafeTrade;
using NeoServer.Game.Systems.SafeTrade.Operations;
using NeoServer.Game.Systems.SafeTrade.Validations;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Game.Systems.Tests.SafeTrade;

public class TradeExchangeValidationTests
{
    [Fact]
    public void Trade_is_cancelled_when_player_has_no_capacity_to_get_a_heavy_weapon()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(capacity: 100);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);

        var item2 = ItemTestData.CreateWeaponItem(1, weight: 150);

        player.Inventory.AddItem(item1, (byte)Slot.Left);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);

        ((DynamicTile)map[101, 100, 7]).AddItem(item2);

        tradeSystem.Request(player, secondPlayer, item1);
        tradeSystem.Request(secondPlayer, player, item2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughCapacity);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    [Fact]
    [ThreadBlocking]
    public void Trade_is_cancelled_when_player_has_no_capacity_to_get_a_heavy_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(capacity: 100);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);

        var backpack = ItemTestData.CreateBackpack(2);
        var weapon = ItemTestData.CreateWeaponItem(1, weight: 100);
        backpack.AddItem(weapon);

        player.Inventory.AddItem(item1, (byte)Slot.Left);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);

        ((DynamicTile)map[101, 100, 7]).AddItem(backpack);

        tradeSystem.Request(player, secondPlayer, item1);
        tradeSystem.Request(secondPlayer, player, backpack);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughCapacity);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_has_no_free_slots()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(capacity: 10000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        Enumerable.Range(0, 20).ToList()
            .ForEach(_ => player.Inventory.BackpackSlot.AddItem(ItemTestData.CreateWeaponItem(1)));

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
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughRoomToCarry);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    [Fact]
    [ThreadBlocking]
    public void Trade_is_cancelled_when_player_has_no_free_slots_and_no_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        inventory.Remove(Slot.Backpack);

        var player = PlayerTestDataBuilder.Build(capacity: 10000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

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
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughRoomToCarry);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_has_no_free_slots_to_carry_cumulative_item()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();

        inventory[Slot.Left] = (ItemTestData.CreateThrowableDistanceItem(1, 50), 1);
        var player = PlayerTestDataBuilder.Build(capacity: 10000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        Enumerable.Range(0, 20).ToList()
            .ForEach(_ => player.Inventory.BackpackSlot.AddItem(ItemTestData.CreateWeaponItem(1)));

        var item1 = ItemTestData.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestData.CreateThrowableDistanceItem(1, 60);

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
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughRoomToCarry);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_has_no_free_slots_and_is_trading_his_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();

        var player = PlayerTestDataBuilder.Build(capacity: 10000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item2 = ItemTestData.CreateWeaponItem(1, weight: 100);

        ((DynamicTile)map[100, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);

        ((DynamicTile)map[100, 100, 7]).AddItem(item2);

        tradeSystem.Request(player, secondPlayer, player.Inventory.BackpackSlot);
        tradeSystem.Request(secondPlayer, player, item2);

        //act
        tradeSystem.AcceptTrade(player);
        var result = tradeSystem.AcceptTrade(secondPlayer);

        //assert
        result.Should().Be(SafeTradeError.PlayerDoesNotHaveEnoughRoomToCarry);
        AssertTradeIsCancelled(tradeSystem, map, player);
    }

    private void AssertTradeIsCancelled(SafeTradeSystem tradeSystem, IMap map, IPlayer player)
    {
        var secondPlayer = PlayerTestDataBuilder.Build();

        var x = (ushort)(player.Location.X + 1);

        var tile = (DynamicTile)map[x, player.Location.Y, player.Location.Z];
        tile.AddCreature(secondPlayer);
        var item = ItemTestData.CreateWeaponItem(1);

        tile.AddItem(item);

        var result = tradeSystem.Request(player, secondPlayer, item);

        result.Should().Be(SafeTradeError.None, "trade is not cancelled");
    }
}