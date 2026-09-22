using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseFactoryTests
{
    [Fact]
    public void Create_FromEntity_MapsIdNameTownRentWarningsOwner()
    {
        var factory = new Domain.Houses.HouseFactory();

        var house = factory.Create(1, "Test House", 2, 500, 10, "Owner", 100, null, 3);

        house.Id.Should().Be(1);
        house.Name.Should().Be("Test House");
        house.TownId.Should().Be(2);
        house.Rent.Should().Be(500u);
        house.OwnerGuid.Should().Be(10);
        house.OwnerName.Should().Be("Owner");
        house.OwnerAccountId.Should().Be(100);
        house.PayRentWarnings.Should().Be(3);
        house.PaidUntil.Should().BeNull();
        house.TileCount.Should().Be(0);
    }

    [Fact]
    public void Create_UnownedEntity_ProducesUnownedHouseWithEmptyLists()
    {
        var factory = new Domain.Houses.HouseFactory();

        var house = factory.Create(1, "Empty", 1, 0, 0, null, 0, null, 0);

        house.OwnerGuid.Should().Be(0);
        house.OwnerName.Should().BeEmpty();
        house.TileCount.Should().Be(0);
        house.DoorCount.Should().Be(0);
        house.BedCount.Should().Be(0);
    }
}
