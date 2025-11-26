using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Combat;

public class PvPCombatTests
{
    [Fact]
    [Trait("Category", "PvP")]
    public void Player_gets_white_skull_when_setting_attack_target_to_unmarked_player_in_normal_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        (map[100, 102, 7] as DynamicTile)?.AddCreature(playerB);

        // Act
        playerAttackService.SetAttackTarget(playerA, playerB);

        // Assert
        playerA.Skull.Should().Be(Skull.White, "Player A should receive white skull when attacking unmarked player");
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should be PZ locked after attacking");
        playerA.HasCondition(ConditionType.LogoutBlock).Should()
            .BeTrue("Player A should have CONDITION_INFIGHT (LogoutBlock)");

        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should see Player A with white skull");
        playerA.PlayerSkull.IsYellowSkull(observer: playerB).Should()
            .BeFalse("Player A should not have yellow skull for Player B");

        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_gets_white_skull_when_attacking_unmarked_player_in_normal_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);
        
        // Act
        playerAttackService.Attack(playerA, playerB);

        // Assert
        playerA.Skull.Should().Be(Skull.White, "Player A should receive white skull when attacking unmarked player");
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should be PZ locked after attacking");
        playerA.HasCondition(ConditionType.LogoutBlock).Should()
            .BeTrue("Player A should have CONDITION_INFIGHT (LogoutBlock)");

        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should see Player A with white skull");
        playerA.PlayerSkull.IsYellowSkull(observer: playerB).Should()
            .BeFalse("Player A should not have yellow skull for Player B");

        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull");
    }
}