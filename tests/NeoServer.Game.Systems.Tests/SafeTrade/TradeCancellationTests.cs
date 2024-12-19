using System.Threading;
using FluentAssertions;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Services;
using NeoServer.Game.Systems.SafeTrade;
using NeoServer.Game.Systems.SafeTrade.Operations;
using NeoServer.Game.Systems.SafeTrade.Validations;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.Tests.Server;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Game.Systems.Tests.SafeTrade;

public class TradeCancellationTests
{
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

    #region Player event cancellation

    [Fact]
    public void Trade_is_cancelled_when_player_moves_2_tiles_away_from_second_player()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateWeaponItem(1);
        ((DynamicTile)map[102, 100, 7]).AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, item);

        player.WalkTo(new Location(104, 100, 7));

        //player will walk 2 steps
        map.MoveCreature(player);
        map.MoveCreature(player);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_logs_out()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateWeaponItem(1);

        //act
        tradeSystem.Request(player, secondPlayer, item);
        player.Logout();

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_dies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateWeaponItem(1);

        //act
        tradeSystem.Request(player, secondPlayer, item);
        player.ReceiveAttack(secondPlayer, new CombatDamage(100, DamageType.Melee));

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_player_moves_away_from_the_traded_item()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[101, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[100, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateWeaponItem(1);
        ((DynamicTile)map[100, 100, 7]).AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, item);
        player.WalkTo(Direction.East, Direction.East);

        map.MoveCreature(player);
        map.MoveCreature(player);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    #endregion

    #region Item movement cancellation

    [Fact]
    public void Trade_is_cancelled_when_item_is_removed_from_tile()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateWeaponItem(1);

        ((DynamicTile)map[101, 100, 7]).AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, item);
        ((DynamicTile)map[101, 100, 7]).RemoveItem(item);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_removed_from_inventory()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = inventory[Slot.Left].Item;

        //act
        tradeSystem.Request(player, secondPlayer, item);
        player.Inventory.RemoveItem(Slot.Left, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_removed_from_inventory_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var backpack = (IContainer)inventory[Slot.Backpack].Item;
        var item = ItemTestData.CreateWeaponItem(1);

        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        backpack.RemoveItem(item, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_added_to_traded_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var backpack = (IContainer)inventory[Slot.Backpack].Item;
        var item = ItemTestData.CreateWeaponItem(1);
        var item3 = ItemTestData.CreateWeaponItem(1);

        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        backpack.AddItem(item3, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_joined_to_traded_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var backpack = (IContainer)inventory[Slot.Backpack].Item;
        var item = ItemTestData.CreateFood(1, 2);
        var item2 = ItemTestData.CreateFood(1, 3);

        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        backpack.AddItem(item2);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_added_to_inner_bag_on_traded_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var backpack = (IContainer)inventory[Slot.Backpack].Item;
        var item = ItemTestData.CreateWeaponItem(1);
        var item3 = ItemTestData.CreateWeaponItem(1);

        var innerBag = ItemTestData.CreateBackpack();

        backpack.AddItem(innerBag);
        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        innerBag.AddItem(item3, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_added_to_traded_backpack_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var backpack = ItemTestData.CreateBackpack();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[100, 100, 7]).AddItem(backpack);

        var item = ItemTestData.CreateWeaponItem(1);
        var item3 = ItemTestData.CreateWeaponItem(1);

        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        backpack.AddItem(item3, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_removed_from_traded_backpack_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var backpack = ItemTestData.CreateBackpack();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[100, 100, 7]).AddItem(backpack);

        var item = ItemTestData.CreateWeaponItem(1);

        backpack.AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        backpack.RemoveItem(item, 1);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, player);
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    #endregion

    #region Item consumption cancellation

    [Fact]
    public void Trade_is_cancelled_when_item_is_consumed_from_tile()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var item = ItemTestData.CreateFood(1, 5);

        ((DynamicTile)map[101, 100, 7]).AddItem(item);

        //act
        tradeSystem.Request(player, secondPlayer, item);
        player.Use(item, player);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_consumed_from_container_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10);
        var secondPlayer = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var backpack = ItemTestData.CreateBackpack();
        var item = ItemTestData.CreateFood(1, 5);

        backpack.AddItem(item);

        ((DynamicTile)map[101, 100, 7]).AddItem(backpack);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        player.Use(item, player);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_consumed_from_inventory()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, inventoryMap: inventory, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var monster = MonsterTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[102, 100, 7]).AddCreature(monster);

        var backpack = (IContainer)player.Inventory[Slot.Backpack];
        var innerBackpack = ItemTestData.CreateBackpack();

        var food = ItemTestData.CreateFood(1, 5);

        innerBackpack.AddItem(food);
        backpack.AddItem(innerBackpack);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        player.Use(food, player);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_split_inside_inventory_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, inventoryMap: inventory, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var monster = MonsterTestDataBuilder.Build();

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[102, 100, 7]).AddCreature(monster);

        var backpack = (IContainer)player.Inventory[Slot.Backpack];
        var innerBackpack = ItemTestData.CreateBackpack();

        var food = ItemTestData.CreateFood(1, 5);

        innerBackpack.AddItem(food);
        backpack.AddItem(innerBackpack);

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        innerBackpack.RemoveItem(0, 3, out var removed);
        innerBackpack.AddItem(removed);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_is_consumed_from_inventory_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, inventoryMap: inventory, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var monster = MonsterTestDataBuilder.Build();

        var distanceWeapon = ItemTestData.CreateThrowableDistanceItem(10, 10, breakChance: 100);

        player.Inventory.AddItem(distanceWeapon);

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[102, 100, 7]).AddCreature(monster);

        //act
        tradeSystem.Request(player, secondPlayer, distanceWeapon);
        player.Attack(monster);

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    #endregion

    #region Item decay cancellation

    [Fact]
    public void Trade_is_cancelled_when_item_decays_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateWeaponItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.DecayTo, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.ExpireTarget, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Duration, 1000);

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddItem(item);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, new ItemTypeStore());

        //act
        tradeSystem.Request(player, secondPlayer, item);
        Thread.Sleep(1010);
        decayableItemManager.DecayExpiredItems();

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_decays_inside_a_backpack_on_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var backpack = ItemTestData.CreateBackpack();

        var item = ItemTestData.CreateWeaponItem(1);
        backpack.AddItem(item);

        item.Metadata.Attributes.SetAttribute(ItemAttribute.DecayTo, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.ExpireTarget, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Duration, 1000);

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);
        ((DynamicTile)map[101, 100, 7]).AddItem(backpack);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, new ItemTypeStore());

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        Thread.Sleep(1010);
        decayableItemManager.DecayExpiredItems();

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_item_decays_inside_the_inventory_backpack()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var backpack = (IContainer)player.Inventory[Slot.Backpack];

        var item = ItemTestData.CreateWeaponItem(1);
        backpack.AddItem(item);

        var innerBackpack = ItemTestData.CreateBackpack();
        innerBackpack.AddItem(item);

        backpack.AddItem(innerBackpack);

        item.Metadata.Attributes.SetAttribute(ItemAttribute.DecayTo, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.ExpireTarget, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Duration, 1000);

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, new ItemTypeStore());

        //act
        tradeSystem.Request(player, secondPlayer, backpack);
        Thread.Sleep(1010);
        decayableItemManager.DecayExpiredItems();

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    [Fact]
    public void Trade_is_cancelled_when_inventory_item_decays()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);

        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(hp: 10, capacity: uint.MaxValue, inventoryMap: inventory);
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item = player.Inventory[Slot.Left];

        item.Metadata.Attributes.SetAttribute(ItemAttribute.DecayTo, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.ExpireTarget, 0);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Duration, 1000);

        ((DynamicTile)map[100, 100, 7]).AddCreature(secondPlayer);
        ((DynamicTile)map[101, 100, 7]).AddCreature(player);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, new ItemTypeStore());

        //act
        tradeSystem.Request(player, secondPlayer, item);
        Thread.Sleep(1010);
        decayableItemManager.DecayExpiredItems();

        //assert
        AssertTradeIsCancelled(tradeSystem, map, secondPlayer);
    }

    #endregion
}