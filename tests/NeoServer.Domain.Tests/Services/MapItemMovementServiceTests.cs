using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Mail;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Services;

public class MapItemMovementServiceTests
{
    // ------------------------------------------------------------------
    // Factory helpers
    // ------------------------------------------------------------------

    private static MapItemMovementService BuildService(NeoServer.Domain.Common.Contracts.World.IMap map)
    {
        var walkTo = new Mock<IWalkToMechanism>();
        var validator = new ItemThrowValidator(map);
        var mail = new Mock<IMailService>();

        return new MapItemMovementService(map, walkTo.Object, validator, mail.Object);
    }

    private static IDynamicTile GetTile(NeoServer.Domain.Common.Contracts.World.IMap map, Location location)
        => (IDynamicTile)map[location];

    // ------------------------------------------------------------------
    // Basic Movement
    // ------------------------------------------------------------------

    [Fact]
    public void Given_item_on_ground_at_100_100_7_When_moved_to_101_100_7_Then_item_appears_at_destination()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        var toTile = GetTile(map, toLocation);

        // WHEN
        var result = sut.Move(player, item, fromTile, toTile, 1, 0, 0);

        // THEN
        result.Succeeded.Should().BeTrue();
        toTile.TopDownItemOnStack.Should().Be(item);
        fromTile.TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_cumulative_item_amount_100_at_100_100_7_When_40_moved_to_101_100_7_Then_40_at_destination_and_60_remain()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateCumulativeItem(100, amount: 100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 40, 0, 0);

        // THEN
        result.Succeeded.Should().BeTrue();
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBeNull();
        GetTile(map, toLocation).TopDownItemOnStack.Amount.Should().Be(40);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBeNull();
        GetTile(map, fromLocation).TopDownItemOnStack.Amount.Should().Be(60);
    }

    [Fact]
    public void Given_cumulative_item_amount_100_at_100_100_7_When_100_moved_to_101_100_7_Then_100_at_destination_and_nothing_remains()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateCumulativeItem(100, amount: 100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 100, 0, 0);

        // THEN
        result.Succeeded.Should().BeTrue();
        GetTile(map, toLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Amount.Should().Be(100);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_unmovable_item_at_100_100_7_When_moved_to_101_100_7_Then_fails_with_NotPossible()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        // CreateRegularItem has no Movable flag
        var item = ItemTestDataBuilder.CreateRegularItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.NotPossible);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    // ------------------------------------------------------------------
    // Distance Validation
    // ------------------------------------------------------------------

    [Fact]
    public void Given_item_at_100_100_7_When_moved_to_107_100_7_distance_7_Then_succeeds()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(107, 100, 7);

        var map = MapTestDataBuilder.Build(99, 115, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN
        result.Succeeded.Should().BeTrue();
        GetTile(map, toLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_item_at_100_100_7_When_moved_to_108_100_7_distance_8_Then_fails_with_TooFar()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(108, 100, 7);

        var map = MapTestDataBuilder.Build(99, 115, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.TooFar);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    // ------------------------------------------------------------------
    // Floor Validation
    // ------------------------------------------------------------------

    [Fact]
    public void Given_item_at_100_100_7_and_player_at_100_100_8_When_moved_Then_fails_with_first_go_upstairs()
    {
        // GIVEN – item on floor 7, player one floor below (Z=8 is deeper underground in Tibia)
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 8);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        // Place the player on the Z=8 tile so their location becomes (100,100,8)
        var player = PlayerTestDataBuilder.Build(map: map);
        player.SetNewLocation(new Location(100, 100, 8));
        map.PlaceCreature(player);

        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN – item.Location.Z (7) < player.Location.Z (8) → "First go upstairs"
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.NotPossible);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_item_at_100_100_8_and_player_at_100_100_7_When_moved_Then_fails_with_first_go_downstairs()
    {
        // GIVEN – item on floor 8 (underground), player one floor above (Z=7)
        var fromLocation = new Location(100, 100, 8);
        var toLocation = new Location(101, 100, 8);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 8);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        // Player stays at default location (100,100,7)
        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN – item.Location.Z (8) > player.Location.Z (7) → "First go downstairs"
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.NotPossible);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_item_at_100_100_7_and_player_at_100_100_7_When_moved_to_101_100_6_different_floor_Then_fails_with_NotPossible()
    {
        // GIVEN – item and player on floor 7, destination on floor 6
        var fromLocation = new Location(100, 100, 7);
        var toLocation = new Location(101, 100, 6);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 6, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        var fromTile = GetTile(map, fromLocation);
        fromTile.AddItem(item);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, fromTile, GetTile(map, toLocation), 1, 0, 0);

        // THEN – ItemThrowValidator: fromLocation.Z != toLocation.Z → NotPossible
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.NotPossible);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    // ------------------------------------------------------------------
    // Sight Validation
    // ------------------------------------------------------------------

    [Fact]
    public void Given_wall_at_101_100_7_blocking_sight_When_item_moved_from_100_100_7_to_103_100_7_Then_fails_with_NotPossible()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var wallLocation = new Location(101, 100, 7);
        var toLocation = new Location(103, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        // A wall with BlockProjectTile blocks the sight ray between from and to
        var wall = ItemTestDataBuilder.CreateRegularItem(200);
        wall.Metadata.Flags.Add(ItemFlag.BlockProjectTile);
        GetTile(map, wallLocation).AddItem(wall);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, toLocation), 1, 0, 0);

        // THEN – SightClear returns false → NotPossible
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.NotPossible);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, toLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    // ------------------------------------------------------------------
    // Special Tile Exemptions
    // ------------------------------------------------------------------

    [Fact]
    public void Given_teleport_at_101_100_7_with_destination_115_100_7_When_item_moved_to_teleport_Then_succeeds_bypassing_distance_check()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var teleportLocation = new Location(101, 100, 7);
        var teleportDestination = new Location(115, 100, 7);

        // Map must cover both teleport location and the teleport's destination tile
        var map = MapTestDataBuilder.Build(99, 120, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        var teleportItem = new TeleportItem(new ItemType().SetFlag(ItemFlag.AlwaysOnTop).SetClientId(10), teleportLocation);
        teleportItem.Attributes.SetAttribute(new Dictionary<ItemAttribute, IConvertible>
        {
            [ItemAttribute.TeleportDestination] = teleportDestination
        });
        GetTile(map, teleportLocation).AddItem(teleportItem);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN – distance from player (100,100,7) to teleport (101,100,7) is 1; teleport bypasses
        //        the >7-tile distance restriction to its destination (115,100,7)
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, teleportLocation), 1, 0, 0);

        // THEN – move succeeds, and the item ends up at the teleport's destination, not the teleport tile
        result.Succeeded.Should().BeTrue();
        GetTile(map, teleportDestination).TopDownItemOnStack.Should().Be(item);
        GetTile(map, teleportLocation).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_circular_teleport_chain_When_item_moved_to_teleport1_Then_item_stays_on_teleport1_tile()
    {
        // GIVEN
        // T1 (101,100,7) → T2 (102,100,7) → T3 (103,100,7) → T1 (101,100,7)  — circular loop
        var fromLocation = new Location(100, 100, 7);
        var t1Location = new Location(101, 100, 7);
        var t2Location = new Location(102, 100, 7);
        var t3Location = new Location(103, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        var itemType = new ItemType().SetFlag(ItemFlag.AlwaysOnTop).SetClientId(10);

        var t1 = new TeleportItem(itemType, t1Location);
        t1.Attributes.SetAttribute(new Dictionary<ItemAttribute, IConvertible>
            { [ItemAttribute.TeleportDestination] = t2Location });
        GetTile(map, t1Location).AddItem(t1);

        var t2 = new TeleportItem(itemType, t2Location);
        t2.Attributes.SetAttribute(new Dictionary<ItemAttribute, IConvertible>
            { [ItemAttribute.TeleportDestination] = t3Location });
        GetTile(map, t2Location).AddItem(t2);

        var t3 = new TeleportItem(itemType, t3Location);
        t3.Attributes.SetAttribute(new Dictionary<ItemAttribute, IConvertible>
            { [ItemAttribute.TeleportDestination] = t1Location });
        GetTile(map, t3Location).AddItem(t3);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, t1Location), 1, 0, 0);

        // THEN – the circular chain is detected; the item lands on T1 and does not loop forever
        result.Succeeded.Should().BeTrue();
        GetTile(map, t1Location).TopDownItemOnStack.Should().Be(item);
        GetTile(map, t2Location).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, t3Location).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_water_at_101_100_7_When_item_moved_to_water_Then_item_is_removed_from_source_and_not_at_destination()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var waterLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        // Replace the tile's ground with water ground (LiquidSource flag)
        var waterType = new ItemType().SetFlag(ItemFlag.LiquidSource).SetClientId(2); 
        var waterGround = new Ground(waterType, waterLocation);
        GetTile(map, waterLocation).ReplaceGround(waterGround);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, waterLocation), 1, 0, 0);

        // THEN – item is consumed by the water: removed from source, not placed on the water tile
        result.Succeeded.Should().BeTrue();
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, waterLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_trashholder_at_101_100_7_When_item_moved_to_trashholder_Then_item_is_removed_from_source_and_not_at_destination()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var trashLocation = new Location(101, 100, 7);

        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 7);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        // A dustbin — AlwaysOnTop item with type="trashholder" — triggers TileFlags.TrashHolder
        var trashType = new ItemType().SetFlag(ItemFlag.AlwaysOnTop).SetClientId(2);
        trashType.Attributes.SetAttribute(ItemTypeAttribute.Type, "trashholder");
        var dustbin = new Item(trashType, trashLocation);
        GetTile(map, trashLocation).AddItem(dustbin);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, trashLocation), 1, 0, 0);

        // THEN – item is consumed: removed from source, not placed on the trash holder tile
        result.Succeeded.Should().BeTrue();
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, trashLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_hole_at_101_100_7_When_item_moved_to_hole_Then_item_ends_up_one_floor_below()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var holeLocation = new Location(101, 100, 7);
        var belowLocation = new Location(101, 100, 8);

        // Map must include floor 8 so the resolved tile exists
        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 8);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        // Replace the ground at holeLocation with a Ground that has FloorChange "down"
        var holeGround = new Ground(new ItemType().SetClientId(2), holeLocation);
        holeGround.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");
        GetTile(map, holeLocation).ReplaceGround(holeGround);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN – player moves an item onto the hole tile
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, holeLocation), 1, 0, 0);

        // THEN – move succeeds and the item falls through to the floor below
        result.Succeeded.Should().BeTrue();
        GetTile(map, belowLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, holeLocation).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Given_hole_at_101_100_7_and_hole_at_101_100_8_When_item_moved_to_hole_Then_item_ends_up_at_101_100_9()
    {
        // GIVEN
        var fromLocation = new Location(100, 100, 7);
        var hole1Location = new Location(101, 100, 7);
        var hole2Location = new Location(101, 100, 8);
        var finalLocation = new Location(101, 100, 9);

        // Map must include floors 7, 8, and 9
        var map = MapTestDataBuilder.Build(99, 110, 99, 110, 7, 9);

        var item = ItemTestDataBuilder.CreateMoveableItem(100);
        GetTile(map, fromLocation).AddItem(item);

        // First hole: 101,100,7 → drops to floor 8
        var holeGround1 = new Ground(new ItemType().SetClientId(2), hole1Location);
        holeGround1.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");
        GetTile(map, hole1Location).ReplaceGround(holeGround1);

        // Second hole: 101,100,8 → drops to floor 9
        var holeGround2 = new Ground(new ItemType().SetClientId(2), hole2Location);
        holeGround2.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");
        GetTile(map, hole2Location).ReplaceGround(holeGround2);

        var player = PlayerTestDataBuilder.Build(map: map);
        var sut = BuildService(map);

        // WHEN – player moves an item onto the first hole tile
        var result = sut.Move(player, item, GetTile(map, fromLocation), GetTile(map, hole1Location), 1, 0, 0);

        // THEN – item falls through both holes and lands two floors down at 101,100,9
        result.Succeeded.Should().BeTrue();
        GetTile(map, finalLocation).TopDownItemOnStack.Should().Be(item);
        GetTile(map, hole2Location).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, hole1Location).TopDownItemOnStack.Should().NotBe(item);
        GetTile(map, fromLocation).TopDownItemOnStack.Should().NotBe(item);
    }
}
