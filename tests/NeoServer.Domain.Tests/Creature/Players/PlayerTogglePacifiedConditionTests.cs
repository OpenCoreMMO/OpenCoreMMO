using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerTogglePacifiedConditionTests
{
    private static IDynamicTile CreateRegularTile(Location location)
    {
        var ground = MapTestDataBuilder.CreateGround(location, 1);
        return new DynamicTile(new Coordinate(location), TileFlag.None, ground, [], []);
    }

    private static IDynamicTile CreateProtectionZoneTile(Location location)
    {
        var ground = MapTestDataBuilder.CreateGround(location, 1);
        return new DynamicTile(new Coordinate(location), (TileFlag)TileFlags.ProtectionZone, ground, [], []);
    }

    [Fact]
    public void Entering_protection_zone_adds_pacified_condition()
    {
        var player = PlayerTestDataBuilder.Build();
        var fromTile = CreateRegularTile(new Location(100, 100, 7));
        var toTile = CreateProtectionZoneTile(new Location(100, 101, 7));

        player.OnMoved(fromTile, toTile, []);

        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Leaving_protection_zone_removes_pacified_condition()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new PacifiedCondition());
        player.IsPacified.Should().BeTrue();

        var fromTile = CreateProtectionZoneTile(new Location(100, 100, 7));
        var toTile = CreateRegularTile(new Location(100, 101, 7));

        player.OnMoved(fromTile, toTile, []);

        player.IsPacified.Should().BeFalse();
    }

    [Fact]
    public void Entering_protection_zone_removes_logout_block()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.LogoutBlock));
        player.IsLogoutBlocked.Should().BeTrue();

        var fromTile = CreateRegularTile(new Location(100, 100, 7));
        var toTile = CreateProtectionZoneTile(new Location(100, 101, 7));

        player.OnMoved(fromTile, toTile, []);

        player.IsLogoutBlocked.Should().BeFalse();
        player.IsPacified.Should().BeTrue();
    }
}
