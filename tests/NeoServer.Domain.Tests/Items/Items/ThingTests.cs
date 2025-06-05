using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items;

public class ThingTests
{
    [Fact]
    public void Item_2_tiles_far_from_player_returns_as_not_closed()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        var item = ItemTestData.CreateWeaponItem(1);

        ((IDynamicTile)map[100, 102, 7]).AddItem(item);

        //act
        var result = item.IsCloseTo(player);

        //assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Item_in_different_floor_from_player_returns_as_not_closed()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        var item = ItemTestData.CreateWeaponItem(1);

        ((IDynamicTile)map[100, 102, 8]).AddItem(item);

        //act
        var result = item.IsCloseTo(player);

        //assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Item_one_tile_from_player_returns_as_closed()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        var item = ItemTestData.CreateWeaponItem(1);

        ((IDynamicTile)map[100, 101, 7]).AddItem(item);

        //act
        var result = item.IsCloseTo(player);

        //assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Item_on_inventory_tile_is_closed_to_player()
    {
        //arrange
        var item = ItemTestData.CreateWeaponItem(1);
        var player = PlayerTestDataBuilder.Build(inventoryMap: new Dictionary<Slot, (IItem Item, ushort Id)>
        {
            [Slot.Left] = new(item, 1)
        });

        //act
        var result = item.IsCloseTo(player);

        //assert
        result.Should().BeTrue();
    }
}