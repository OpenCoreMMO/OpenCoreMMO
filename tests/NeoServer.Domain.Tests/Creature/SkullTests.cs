using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature;

public class SkullTests
{
    [Fact]
    [Trait("Category", "Skull")]
    public void Player_gets_white_skull_when_attacking_summon_of_another_player()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var attackService = AttackServiceTestBuilder.Build(map);

        var aggressor = PlayerTestDataBuilder.Build(1, "Aggressor");
        aggressor.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(aggressor);

        var summonMaster = PlayerTestDataBuilder.Build(2, "SummonMaster");

        var summon = MonsterTestDataBuilder.BuildSummon(summonMaster);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(summon);

        var attackInput = new AttackInput(aggressor, summon, PlayerCombatParameterBuilder.Build(aggressor, summon));

        // Act
        attackService.Execute(attackInput);

        // Assert
        aggressor.Skull.Should().Be(Skull.White);
        summonMaster.Skull.Should().Be(Skull.None);
    }
}