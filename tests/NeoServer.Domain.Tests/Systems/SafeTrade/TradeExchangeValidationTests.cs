using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Services;
using NeoServer.Domain.SafeTrade;
using NeoServer.Domain.SafeTrade.Operations;
using NeoServer.Domain.SafeTrade.Validations;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Systems.SafeTrade;

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

        var item1 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);

        var item2 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 150);

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

        var item1 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);

        var backpack = ItemTestDataBuilder.CreateBackpack(2);
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);
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
            .ForEach(_ => player.Inventory.BackpackSlot.AddItem(ItemTestDataBuilder.CreateWeaponItem(1)));

        var item1 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);

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

        var item1 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);

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

        inventory[Slot.Left] = (ItemTestDataBuilder.CreateThrowableDistanceItem(1, 50), 1);
        var player = PlayerTestDataBuilder.Build(capacity: 10000, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        Enumerable.Range(0, 20).ToList()
            .ForEach(_ => player.Inventory.BackpackSlot.AddItem(ItemTestDataBuilder.CreateWeaponItem(1)));

        var item1 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);
        var item2 = ItemTestDataBuilder.CreateThrowableDistanceItem(1, 60);

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

        var item2 = ItemTestDataBuilder.CreateWeaponItem(1, weight: 100);

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
        var item = ItemTestDataBuilder.CreateWeaponItem(1);

        tile.AddItem(item);

        var result = tradeSystem.Request(player, secondPlayer, item);

        result.Should().Be(SafeTradeError.None, "trade is not cancelled");
    }
}