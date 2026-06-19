using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Houses.AccessList;

namespace NeoServer.Domain.Tests.Houses;

public class HouseDoorBedTests
{
    [Fact]
    public void GetDoorCount_AfterLinking_ReturnsCount()
    {
        var house = HouseTestDataBuilder.Build();
        house.LinkDoor(1, new Mock<IItem>().Object);
        house.LinkDoor(2, new Mock<IItem>().Object);

        house.DoorCount.Should().Be(2);
    }

    [Fact]
    public void LinkBed_AddsBedToHouse_GetBedCountReflects()
    {
        var house = HouseTestDataBuilder.Build();
        house.LinkBed(new Mock<IItem>().Object);
        house.LinkBed(new Mock<IItem>().Object);

        house.BedCount.Should().Be(2);
    }

    [Fact]
    public void SetAccessList_DoorListId_UpdatesThatDoorOnly()
    {
        var list1 = HouseTestDataBuilder.CreateAccessList("PlayerA");
        var list2 = HouseTestDataBuilder.CreateAccessList("PlayerB");

        var house = HouseTestDataBuilder.Build();
        house.SetAccessList(1, list1);
        house.SetAccessList(2, list2);

        var newListForDoor1 = HouseTestDataBuilder.CreateAccessList("PlayerC");
        house.SetAccessList(1, newListForDoor1);

        house.GetAccessList(1).Should().BeSameAs(newListForDoor1);
        house.GetAccessList(2).Should().BeSameAs(list2);
    }

    [Fact]
    public void EntryPosition_DefaultsToFirstTileLocation()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();

        house.LinkTile(tileMock.Object);

        house.EntryPosition.Should().Be(tileMock.Object.Location);
    }
}
