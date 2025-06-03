using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Items;

public class PickupableContainerTest
{
    [Fact]
    public void Weight_When_Add_Item_Returns_Increased_Weight()
    {
        var container = ItemTestData.CreatePickupableContainer();
        Assert.Equal(20, container.Weight);

        container.AddItem(ItemTestData.CreateBodyEquipmentItem(100, "", "shield"));
        container.AddItem(ItemTestData.CreateBodyEquipmentItem(101, "", "shield"));

        Assert.Equal(100, container.Weight);
    }

    [Fact]
    public void Weight_When_Remove_Item_Returns_Decreased_Weight()
    {
        var container = ItemTestData.CreatePickupableContainer();
        Assert.Equal(20, container.Weight);

        container.AddItem(ItemTestData.CreateBodyEquipmentItem(100, "", "shield"));
        Assert.Equal(60, container.Weight);

        container.RemoveItem(0, 1, out var removed);

        Assert.Equal(20, container.Weight);
    }

    [Fact]
    public void Weight_When_Add_Cumulative_Item_Returns_Increased_Weight()
    {
        var container = ItemTestData.CreatePickupableContainer();
        Assert.Equal(20, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 30));
        Assert.Equal(50, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 10));
        Assert.Equal(60, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 70));
        Assert.Equal(130, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(105, 70));
        Assert.Equal(200, container.Weight);
    }

    [Fact]
    public void Weight_When_Remove_Cumulative_Item_Returns_Increased_Weight()
    {
        var container = ItemTestData.CreatePickupableContainer();
        Assert.Equal(20, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 30));
        Assert.Equal(50, container.Weight);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 10));
        Assert.Equal(60, container.Weight);
    }

    [Fact]
    public void Weight_When_Add_Item_To_Child_Container_Returns_Increased_Weight()
    {
        var sut = ItemTestData.CreatePickupableContainer();
        var child = ItemTestData.CreatePickupableContainer();

        sut.AddItem(child);

        child.AddItem(ItemTestData.CreateBodyEquipmentItem(100, "", "shield"));

        Assert.Equal(80, sut.Weight);
    }

    [Fact]
    public void Weight_When_Remove_Child_Container_Returns_Decreased_Weight()
    {
        var sut = ItemTestData.CreatePickupableContainer();
        var child = ItemTestData.CreatePickupableContainer();

        sut.AddItem(child);

        var shield = ItemTestData.CreateBodyEquipmentItem(100, "", "shield");
        child.AddItem(shield);

        Assert.Equal(80, sut.Weight);

        child.RemoveItem(0, 1, out var removed);

        Assert.Equal(40, sut.Weight);

        sut.RemoveItem(0, 1, out var removed2);
        Assert.Equal(20, sut.Weight);
    }

    [Fact]
    public void Weight_When_Remove_Child_Container_And_Add_Item_Returns_Same_Weight()
    {
        //arrange
        var sut = ItemTestData.CreatePickupableContainer();
        var child = ItemTestData.CreatePickupableContainer();

        sut.AddItem(child);

        var shield = ItemTestData.CreateBodyEquipmentItem(100, "", "shield");

        //act
        child.AddItem(shield);

        sut.RemoveItem(0, 1, out var removed);

        //assert
        Assert.Equal(20, sut.Weight);

        //act
        child.AddItem(ItemTestData.CreateBodyEquipmentItem(102, "", "shield"));

        //assert
        Assert.Equal(20, sut.Weight);
    }

    [Fact]
    public void Weight_When_Move_Child_Container_And_Add_Item_Returns_Increased_Weight()
    {
        var player = PlayerTestDataBuilder.Build();

        IDynamicTile tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());

        var sut = ItemTestData.CreatePickupableContainer();
        var child = ItemTestData.CreatePickupableContainer();
        var child2 = ItemTestData.CreatePickupableContainer();

        tile.AddItem(sut);

        sut.AddItem(child);
        sut.AddItem(child2);

        var shield = ItemTestData.CreateBodyEquipmentItem(100, "", "shield");
        child.AddItem(shield);

        player.MoveItem(child, sut, sut, 1, 1, 0);

        Assert.Equal(100, sut.Weight);

        child.AddItem(ItemTestData.CreateBodyEquipmentItem(102, "", "shield"));

        Assert.Equal(140, sut.Weight);

        child2.AddItem(ItemTestData.CreateBodyEquipmentItem(104, "", "shield"));

        Assert.Equal(180, sut.Weight);

        child2.RemoveItem(1, 1, out var removed);

        Assert.Equal(80, sut.Weight);
    }
}