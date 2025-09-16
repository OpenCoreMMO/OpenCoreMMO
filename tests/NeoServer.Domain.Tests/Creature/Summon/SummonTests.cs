using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Summon;

public class SummonTests
{
    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_does_not_attack_player_B_when_master_player_A_has_no_target_and_moves_to_another_floor()
    {
        // Arrange
        // Create a map with multiple floors to test floor change
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 6, 7, true);
        
        // Create master player A at position (105, 105, 7) - floor 7
        var master = PlayerTestDataBuilder.Build(
            id: 1,
            name: "PlayerA",
            hp: 150);
        
        // Create player B (potential target) at position (106, 105, 6) - floor 6
        var playerB = PlayerTestDataBuilder.Build(
            id: 2,
            name: "PlayerB",
            hp: 150);
        
        // Create summon for master at position (104, 105, 7) - floor 7
        var summon = MonsterTestDataBuilder.BuildSummon(master, 50, 100);
        
        // Place creatures on the map
        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[104, 105, 7] as DynamicTile)?.AddCreature(summon);
        (map[106, 105, 7] as DynamicTile)?.AddCreature(playerB);
        
        // Act
        // Move master to a position that triggers floor change to floor 6
        var success = map.TryMoveCreature(master, new Location(105, 105, 6));
        
        summon.SetAsEnemy(playerB);
        
        // Assert
        // The summon should not automatically attack player B just because master changed floors
        // The summon only attacks when the master has a target
        summon.Attacking.Should().BeFalse();
        summon.AutoAttackTargetId.Should().Be(0);
    }
}