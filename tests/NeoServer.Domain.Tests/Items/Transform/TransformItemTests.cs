using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;

namespace NeoServer.Domain.Tests.Items.Container;

public class TransformItemTests
{
    [Fact]
    public void Transform_item_inside_container_to_another_item()
    {
        //arrange
        var container = ItemTestDataBuilder.CreateContainer(5);
        var player = PlayerTestDataBuilder.Build();

        ushort idItem1 = 101;
        ushort idItem2 = 102;
        ushort idItem3 = 103;

        var item1 = ItemTestDataBuilder.CreateWeaponItem(idItem1);
        var item2 = ItemTestDataBuilder.CreateWeaponItem(idItem2);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(idItem3);

        container.AddItem(item1);
        container.AddItem(item2);
        player.Inventory.AddItem(container, Slot.Backpack);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();
        ItemTestDataBuilder.AddItemTypeStore(itemTypeStore, item1.Metadata, item2.Metadata, item3.Metadata);

        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);
        map.PlaceCreature(player);
        var transformService = ItemTransformServiceTestBuilder.Build(map, itemTypeStore);

        //act
        transformService.Transform(player, item1, idItem3);

        //assert
        container.GetFirstItemByServerId(idItem1).ItemFound.Should().BeNull();
        container.GetFirstItemByServerId(idItem2).ItemFound.Should().NotBeNull();
        container.GetFirstItemByServerId(idItem3).ItemFound.Should().NotBeNull();
    }

    [Fact]
    public void Transform_item_inside_inventory_to_another_item()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        ushort idItem1 = 101;
        ushort idItem2 = 102;
        ushort idItem3 = 103;

        var item1 = ItemTestDataBuilder.CreateBodyEquipmentItem(idItem1, "feet");
        var item2 = ItemTestDataBuilder.CreateBodyEquipmentItem(idItem2, "head");
        var item3 = ItemTestDataBuilder.CreateBodyEquipmentItem(idItem3, "feet");

        player.Inventory.AddItem(item1, Slot.Feet);
        player.Inventory.AddItem(item2, Slot.Head);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();
        ItemTestDataBuilder.AddItemTypeStore(itemTypeStore, item1.Metadata, item2.Metadata, item3.Metadata);

        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);
        map.PlaceCreature(player);
        var transformService = ItemTransformServiceTestBuilder.Build(map, itemTypeStore);

        //act
        transformService.Transform(player, item1, idItem3);

        //assert
        var feetItem = player.Inventory.TryGetItem<IItem>(Slot.Feet);
        var headItem = player.Inventory.TryGetItem<IItem>(Slot.Head);

        feetItem.Should().NotBeNull();
        feetItem.ServerId.Should().Be(idItem3);
        headItem.Should().NotBeNull();
        headItem.ServerId.Should().Be(idItem2);
    }

    [Fact]
    public void Transform_item_inside_ground_to_another_item()
    {
        //arrange
        ushort idItem1 = 101;
        ushort idItem2 = 102;
        ushort idItem3 = 103;

        var item1 = ItemTestDataBuilder.CreateWeaponItem(idItem1);
        var item2 = ItemTestDataBuilder.CreateWeaponItem(idItem2);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(idItem3);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();
        ItemTestDataBuilder.AddItemTypeStore(itemTypeStore, item1.Metadata, item2.Metadata, item3.Metadata);

        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);
        var location = new Location(100, 100, 7);

        var tile = map.GetTile(location);
        var dynamicTile = tile as IDynamicTile;

        dynamicTile.AddItem(item1);
        dynamicTile.AddItem(item2);

        var transformService = ItemTransformServiceTestBuilder.Build(map, itemTypeStore);

        //act
        transformService.Transform(item1, idItem3);

        //assert
        dynamicTile.AllItems.FirstOrDefault(c => c.ServerId == idItem1).Should().BeNull();
        dynamicTile.AllItems.FirstOrDefault(c => c.ServerId == idItem2).Should().NotBeNull();
        dynamicTile.AllItems.FirstOrDefault(c => c.ServerId == idItem3).Should().NotBeNull();
    }
}