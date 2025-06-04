using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerMoveItemTests
{
    #region Inventory tests

    [Fact]
    public void Player_moves_item_from_ground_to_inventory()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var item = ItemTestData.CreateWeaponItem(100);

        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(item, tile, inventory, 1, 0, (byte)Slot.Left);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal(inventory[Slot.Left], item);
        Assert.Null(tile.TopItemOnStack);
    }


    [Fact]
    public void Player_cannot_move_weapon_to_backpack_slot()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var item = ItemTestData.CreateWeaponItem(100);

        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(item, tile, inventory, 1, 0, (byte)Slot.Backpack);

        //assert
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.CannotDress);
        inventory[Slot.Backpack].Should().BeNull();
        tile.TopItemOnStack.Should().Be(item);
    }


    [Fact]
    public void Player_swaps_item_from_ground_to_inventory()
    {
        //arrange
        var dictionary = new Dictionary<Slot, (IItem Item, ushort Id)>();

        var itemOnInventory = ItemTestData.CreateWeaponItem(200, "axe");
        dictionary.Add(Slot.Left, (itemOnInventory, 200));

        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var inventory = InventoryTestDataBuilder.Build(player, dictionary);
        var item = ItemTestData.CreateWeaponItem(100);
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(item, tile, inventory, 1, 0, (byte)Slot.Left);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal(inventory[Slot.Left], item);
        Assert.Equal(itemOnInventory, tile.TopItemOnStack);
    }

    [Fact]
    public void Player_moves_cumulative_from_ground_to_inventory()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var item = ItemTestData.CreateAmmo(100, 20);
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(tile.TopItemOnStack, tile, inventory, 20, 0, (byte)Slot.Ammo);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal(20, inventory[Slot.Ammo].Amount);

        Assert.Null(tile.TopItemOnStack);
    }

    [Fact]
    public void Player_moves_cumulative_from_ground_to_exiting_item_on_inventory()
    {
        //arrange
        var itemOnInventory = ItemTestData.CreateAmmo(100, 51);

        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Ammo, (itemOnInventory, 100) }
            });

        var item = ItemTestData.CreateAmmo(100, 100);
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(tile.TopItemOnStack, tile, inventory, 100, 0, (byte)Slot.Ammo);

        //assert
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result.Error);
        Assert.Equal(100, inventory[Slot.Ammo].Amount);

        Assert.Equal(51, tile.TopItemOnStack.Amount);
    }


    [Fact]
    public void Player_cannot_move_cumulative_from_ground_to_inventory_when_full()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var itemOnInventory = ItemTestData.CreateWeaponItem(100);
        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Left, (itemOnInventory, 100) }
            });

        var item = ItemTestData.CreateThrowableDistanceItem(200, 100);
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new IItem[] { item });

        //act
        var result = player.MoveItem(tile.TopItemOnStack, tile, inventory, 100, 0, (byte)Slot.Ammo);

        //assert
        Assert.False(result.Succeeded);
        Assert.Equal(itemOnInventory, inventory[Slot.Left]);

        Assert.Equal(item, tile.TopItemOnStack);
        Assert.Equal(100, tile.TopItemOnStack.Amount);
    }

    [Fact]
    public void Player_moves_cumulative_from_ground_to_inventory_backpack()
    {
        //arrange
        var backpack = ItemTestData.CreateBackpack();

        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Backpack, (backpack, 100) }
            });

        var item = ItemTestData.CreateAmmo(200, 100);
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(tile.TopItemOnStack, tile, inventory, 100, 0, (byte)Slot.Backpack);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal(item, ((IContainer)inventory[Slot.Backpack])[item.Location.ContainerSlot]);

        Assert.Null(tile.TopItemOnStack);
        Assert.Equal(100, ((IContainer)inventory[Slot.Backpack])[item.Location.ContainerSlot].Amount);
    }

    [Fact]
    public void Player_moves_item_from_inventory_to_tile()
    {
        //arrange
        var item = ItemTestData.CreateBodyEquipmentItem(100, "", "shield");

        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Right, (item, 100) }
            });
        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { item });

        //act
        var result = player.MoveItem(item, inventory, tile, 1, (byte)Slot.Right, 0);

        //assert
        Assert.True(result.Succeeded);
        Assert.Null(inventory[Slot.Right]);
        Assert.Equal(item, tile.TopItemOnStack);
    }

    [Fact]
    public void Player_moves_ammo_from_container_to_inventory()
    {
        //arrange
        var container = ItemTestData.CreateContainer(2);
        var ammo = ItemTestData.CreateAmmo(100, 1);
        container.AddItem(ammo);

        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = player.MoveItem(ammo, container, inventory, 1, (byte)ammo.Location.ContainerSlot, (byte)Slot.Ammo);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal((uint)2, container.FreeSlotsCount);
        Assert.Equal(ammo, inventory[Slot.Ammo]);
        Assert.Equal(1, inventory[Slot.Ammo].Amount);
    }

    [Fact]
    public void Player_moves_ammo_from_backpack_to_inventory()
    {
        //arrange
        var backpack = ItemTestData.CreateBackpack();
        var ammo = ItemTestData.CreateAmmo(100, 1);
        backpack.AddItem(ammo);

        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var inventory = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Backpack, (backpack, 100) }
            });

        //act
        var result = player.MoveItem(ammo, backpack, inventory, 1, (byte)ammo.Location.ContainerSlot, (byte)Slot.Ammo);

        //assert
        Assert.True(result.Succeeded);
        Assert.Equal((uint)20, backpack.FreeSlotsCount);
        Assert.Equal(ammo, inventory[Slot.Ammo]);
        Assert.Equal(1, inventory[Slot.Ammo].Amount);
    }

    #endregion

    #region Tile tests

    [Fact]
    public void Player_moves_item_from_tile_to_another_tile()
    {
        //arrange
        IDynamicTile fromTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        IDynamicTile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());

        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var item = ItemTestData.CreateWeaponItem(100);
        fromTile.AddItem(item);

        //act
        var result = player.MoveItem(item, fromTile, dest, 1, 0, 0);

        //assert
        Assert.True(result.Succeeded);
        Assert.Null(fromTile.TopItemOnStack);
        Assert.Equal(item, dest.TopItemOnStack);
    }

    [Fact]
    public void Player_moves_cumulative_item_from_tile_to_another_tile()
    {
        //arrange
        IDynamicTile fromTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        IDynamicTile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var item = ItemTestData.CreateAmmo(100, 100);

        fromTile.AddItem(item);

        //act
        var result = player.MoveItem(item, fromTile, dest, 80, 0, 0);

        //assert
        Assert.True(result.Succeeded);

        Assert.Equal(100, dest.TopItemOnStack.ClientId);
        Assert.Equal(item, fromTile.TopItemOnStack);

        Assert.Equal(20, ((ICumulative)fromTile.TopItemOnStack).Amount);
        Assert.Equal(80, ((ICumulative)dest.TopItemOnStack).Amount);
    }

    [Fact]
    public void Player_moves_half_of_cumulative_item_from_tile_to_another_tile()
    {
        //arrange
        IDynamicTile fromTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        IDynamicTile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var item = ItemTestData.CreateAmmo(100, 100);

        fromTile.AddItem(item);

        //act
        var result = player.MoveItem(item, fromTile, dest, 50, 0, 0);

        //assert
        Assert.True(result.Succeeded);

        Assert.Equal(100, dest.TopItemOnStack.ClientId);
        Assert.Equal(item, fromTile.TopItemOnStack);

        Assert.Equal(50, ((ICumulative)fromTile.TopItemOnStack).Amount);
        Assert.Equal(50, ((ICumulative)dest.TopItemOnStack).Amount);
    }

    [Fact]
    public void Player_moves_part_of_cumulative_item_from_tile_to_join_in_another_tile()
    {
        //arrange
        IDynamicTile fromTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        IDynamicTile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            new[] { ItemTestData.CreateAmmo(100, 50) });
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var item = ItemTestData.CreateAmmo(100, 100);
        fromTile.AddItem(item);

        //act
        var result = player.MoveItem(item, fromTile, dest, 40, 0, 0);

        //assert
        Assert.True(result.Succeeded);

        Assert.Equal(100, dest.TopItemOnStack.ClientId);
        Assert.Equal(item, fromTile.TopItemOnStack);

        Assert.Equal(60, ((ICumulative)fromTile.TopItemOnStack).Amount);
        Assert.Equal(90, ((ICumulative)dest.TopItemOnStack).Amount);

        //act
        result = player.MoveItem(item, fromTile, dest, 60, 0, 0);

        //assert
        Assert.True(result.Succeeded);

        Assert.Equal(100, dest.TopItemOnStack.ClientId);
        Assert.Null(fromTile.TopItemOnStack);

        Assert.Equal(50, ((ICumulative)dest.TopItemOnStack).Amount);
    }

    #endregion

    #region Container tests

    [Fact]
    public void Player_adds_cumulative_item_to_child_container_joins_or_return_full_when_exceeds()
    {
        //arrange
        var fromContainer = ItemTestData.CreateContainer(2);

        var child = ItemTestData.CreateContainer(1);
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());

        var itemOnChild = ItemTestData.CreateCumulativeItem(100, 50);
        child.AddItem(itemOnChild);

        var item = ItemTestData.CreateCumulativeItem(100, 100);
        fromContainer.AddItem(child);
        fromContainer.AddItem(item);

        tile.AddItem(fromContainer);

        //act
        var result = player.MoveItem(item, fromContainer, child, 70, (byte)item.Location.ContainerSlot,
            (byte)child.Location.ContainerSlot);

        //assert
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.IsFull, result.Error);

        Assert.Equal(100, itemOnChild.Amount);
        Assert.Equal(50, item.Amount);
    }

    [Fact]
    public void Player_moves_cumulative_item_from_container_to_child()
    {
        //arrange
        var fromContainer = ItemTestData.CreateContainer(2);
        var child = ItemTestData.CreateContainer(1);
        var item = ItemTestData.CreateCumulativeItem(100, 40);
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        fromContainer.AddItem(child);
        fromContainer.AddItem(item);

        var eventCalled = false;
        var childEventCalled = false;

        fromContainer.OnItemRemoved += (_, _, _, _) => { eventCalled = true; };
        child.OnItemAdded += (_, _) => { childEventCalled = true; };

        //act
        player.MoveItem(item, fromContainer, child, 40, (byte)item.Location.ContainerSlot,
            (byte)child.Location.ContainerSlot);

        //assert
        Assert.True(eventCalled);
        Assert.True(childEventCalled);

        Assert.Single(fromContainer.Items);
        Assert.Equal(40, ((ICumulative)child[0]).Amount);
    }

    [Fact]
    public void Player_moves_regular_item_from_container_to_child()
    {
        //arrange
        var fromContainer = ItemTestData.CreateBackpack();
        var child = ItemTestData.CreateBackpack();

        var item2 = ItemTestData.CreateBodyEquipmentItem(101, "head");
        var item = ItemTestData.CreateBodyEquipmentItem(100, "head");

        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        fromContainer.AddItem(child);
        fromContainer.AddItem(item);
        fromContainer.AddItem(item2);

        var eventCalled = false;
        var childEventCalled = false;

        fromContainer.OnItemRemoved += (_, _, _, _) => { eventCalled = true; };
        child.OnItemAdded += (_, _) => { childEventCalled = true; };

        //act
        player.MoveItem(item, fromContainer, child, 1, (byte)item.Location.ContainerSlot,
            (byte)child.Location.ContainerSlot);

        //assert
        Assert.True(eventCalled);
        Assert.True(childEventCalled);

        fromContainer.Items.Should().HaveCount(2);
        fromContainer.Items.Should().Contain(item2);
        fromContainer.Items.Should().Contain(child);

        child.Items.Should().Contain(item);
        child.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Player_cannot_move_cumulative_to_child_when_it_is_already_full()
    {
        //arrange
        var fromContainer = ItemTestData.CreateContainer(2);
        var child = ItemTestData.CreateContainer(1);
        var item = ItemTestData.CreateCumulativeItem(100, 40);
        var item2 = ItemTestData.CreateCumulativeItem(100, 100);

        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        fromContainer.AddItem(child);
        fromContainer.AddItem(item);
        child.AddItem(item2);

        //act
        var result = player.MoveItem(item, fromContainer, child, 40, (byte)item.Location.ContainerSlot,
            (byte)child.Location.ContainerSlot);

        //assert
        Assert.Equal(InvalidOperation.IsFull, result.Error);
        Assert.Equal(2, fromContainer.Items.Count);
        Assert.Equal(100, ((ICumulative)child[0]).Amount);
    }

    [Fact]
    public void Player_moves_backpack_to_first_slot_of_another_backpack()
    {
        //arrange
        var anotherBackpack = ItemTestData.CreateContainer(2);
        var bp1 = ItemTestData.CreateContainer(2);
        var item = ItemTestData.CreateBackpack();
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        bp1.AddItem(item);

        //act
        player.MoveItem(item, bp1, anotherBackpack, 1, 0, 0);

        //assert
        Assert.Single(anotherBackpack.Items);
        Assert.Equal(item, anotherBackpack[0]);
    }

    [Fact]
    public void Player_moves_cumulative_item_from_container_to_join_child()
    {
        //arrange
        var fromContainer = ItemTestData.CreateBackpack();
        var child = ItemTestData.CreateBackpack();
        var item = ItemTestData.CreateCumulativeItem(100, 40);
        var item2 = ItemTestData.CreateCumulativeItem(100, 20);
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        fromContainer.AddItem(child);
        fromContainer.AddItem(item);
        child.AddItem(item2);

        var eventCalled = false;
        var childEventCalled = false;

        fromContainer.OnItemUpdated += (_, _, _, _) => { eventCalled = true; };
        child.OnItemUpdated += (_, _, _, _) => { childEventCalled = true; };

        //act
        player.MoveItem(item, fromContainer, child, 20, (byte)item.Location.ContainerSlot,
            (byte)child.Location.ContainerSlot);

        //assert
        Assert.True(eventCalled);
        Assert.True(childEventCalled);

        Assert.Equal(2, fromContainer.Items.Count);
        Assert.Equal(20, ((ICumulative)fromContainer[(byte)item.Location.ContainerSlot]).Amount);
        Assert.Equal(40, ((ICumulative)child[(byte)item2.Location.ContainerSlot]).Amount);
    }

    #endregion
}