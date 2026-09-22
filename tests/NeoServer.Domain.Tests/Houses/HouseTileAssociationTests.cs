using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseTileAssociationTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void LinkTile_InstallsCanEnterFunction_BlockingUninvitedPlayer()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();

        house.LinkTile(tileMock.Object);

        var uninvited = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var canEnter = tileMock.Object.CanEnterFunction(uninvited);
        canEnter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void LinkTile_CanEnterFunction_AllowsInvitedPlayer()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        house.LinkTile(tileMock.Object);

        var owner = HouseTestDataBuilder.CreatePlayer(id: 1);
        var canEnter = tileMock.Object.CanEnterFunction(owner);
        canEnter.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void LinkTile_NonPlayerCreature_CanEnterFalse()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();
        house.LinkTile(tileMock.Object);

        var creatureMock = new Mock<ICreature>();
        var canEnter = tileMock.Object.CanEnterFunction(creatureMock.Object);
        canEnter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void LinkTile_CanEnterFunction_AllowsPlayerWithAdminGroup()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();
        house.LinkTile(tileMock.Object);

        var adminGroup = new Group { Id = 6, Name = "god", Access = true };
        var adminPlayer = HouseTestDataBuilder.CreatePlayer(name: "God", group: adminGroup);
        var canEnter = tileMock.Object.CanEnterFunction(adminPlayer);
        canEnter.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void LinkTile_CanEnterFunction_BlocksPlayerWithNonAdminGroup()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();
        house.LinkTile(tileMock.Object);

        var normalGroup = new Group { Id = 1, Name = "player", Access = false };
        var normalPlayer = HouseTestDataBuilder.CreatePlayer(name: "Regular", group: normalGroup);
        var canEnter = tileMock.Object.CanEnterFunction(normalPlayer);
        canEnter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetTileCount_AfterLinkingTiles_ReturnsCount()
    {
        var house = HouseTestDataBuilder.Build();

        house.LinkTile(HouseTestDataBuilder.CreateTileMock().Object);
        house.LinkTile(HouseTestDataBuilder.CreateTileMock().Object);

        house.TileCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void LinkTile_SetsProtectionZoneFlag()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();

        house.LinkTile(tileMock.Object);

        tileMock.Object.HasFlag(TileFlags.ProtectionZone).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public void LinkTile_SameTileToSameHouseTwice_Throws()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var house = HouseTestDataBuilder.Build();
        house.LinkTile(tileMock.Object);

        Action act = () => house.LinkTile(tileMock.Object);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public void LinkTile_SameTileToTwoDifferentHouses_Throws()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var houseA = HouseTestDataBuilder.Build(id: 1);
        var houseB = HouseTestDataBuilder.Build(id: 2);

        houseA.LinkTile(tileMock.Object);

        // CanEnterFunction is now set by houseA; houseB must reject this tile.
        Action act = () => houseB.LinkTile(tileMock.Object);
        act.Should().Throw<InvalidOperationException>();
    }
}
