using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Creatures;
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

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_does_not_get_skull_when_retaliating_against_white_skull_attacker()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);

        // Act - Step 1: Player A attacks Player B
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 1
        playerA.Skull.Should().Be(Skull.White, "Player A should have white skull after attacking unmarked Player B");
        playerB.Skull.Should().Be(Skull.None, "Player B should have no skull after being attacked");

        // Act - Step 2: Player B retaliates and attacks Player A
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 2 - Key assertions for retaliation
        playerB.Skull.Should().Be(Skull.None, "Player B should NOT receive any skull when retaliating against white skull attacker");
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        
        playerA.Skull.Should().Be(Skull.White, "Player A should still have white skull after being attacked by victim");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should still see Player A with white skull");
        
        // Verify PZ lock and combat conditions are applied correctly
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should remain PZ locked");
        playerB.IsProtectionZoneBlocked.Should().BeTrue("Player B should be PZ locked after retaliating");
        
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_does_not_get_skull_when_retaliating_after_logout_and_login()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);

        // Act - Step 1: Player A attacks Player B
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 1
        playerA.Skull.Should().Be(Skull.White, "Player A should have white skull after attacking unmarked Player B");
        playerB.Skull.Should().Be(Skull.None, "Player B should have no skull after being attacked");

        // Act - Step 2: Player B logs out and logs back in
        playerB.Logout(forced: true);
        
        // Verify logout cleared yellow skull state (yellow skull is removed on logout)
        playerB.Skull.Should().Be(Skull.None, "Player B should have no skull after logout");
        
        // Simulate login
        playerB.Login();

        // Assert after Step 2 - Player B state after re-login
        playerB.Skull.Should().Be(Skull.None, "Player B should return with no skull after re-login");
        playerB.PlayerSkull.IsYellowSkull(playerA).Should().BeFalse("Player B should not have yellow skull for Player A after re-login");

        // Act - Step 3: After logging in, Player B attacks Player A
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 3 - Key assertions for retaliation after re-login
        playerB.Skull.Should().Be(Skull.None, "Player B should NOT receive any skull when attacking after re-login");
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        
        playerA.Skull.Should().Be(Skull.White, "Player A should still have white skull from initial attack");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should still see Player A with white skull");
        
        // Verify PZ lock and combat conditions are applied correctly
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should remain PZ locked");
        playerB.IsProtectionZoneBlocked.Should().BeTrue("Player B should be PZ locked after attacking");
        
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition after attacking");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_gets_yellow_skull_visible_only_to_killer_when_retaliating_after_death_and_login()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1, hp: 100);
        
        for (var i = 1; i <= 100; i++)
        {
            playerA.IncreaseSkillCounter(SkillType.Fist, long.MaxValue);
        }

        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1, hp: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);

        // Create a third player (observer) to test yellow skull visibility
        var playerC = PlayerTestDataBuilder.Build(id: 3, name: "PlayerC", level: 10, experience: 1000, vocationType: 1);
        (map[100, 102, 7] as DynamicTile)?.AddCreature(playerC);

        // Act - Step 1: Player A attacks and kills Player B
        playerAttackService.Attack(playerA, playerB);
        
        // Simulate killing Player B by dealing with enough damage
        while (playerB.HealthPoints > 0)
        {
            playerAttackService.Attack(playerA, playerB);
        }

        // Assert after Step 1 - Death state
        playerA.Skull.Should().Be(Skull.White, "Player A should have white skull after killing Player B");
        playerB.IsDead.Should().BeTrue("Player B should be dead");
        
        // Death should have cleared yellow skull tracking for Player B
        playerB.PlayerSkull.IsYellowSkull(playerA).Should().BeFalse("Player B should not have yellow skull after death");

        // Act - Step 2: Player B logs back in
        // First respawn/revive Player B
        playerB.Heal(100, playerB);
        playerB.Login();

        // Assert after Step 2 - Player B state after re-login
        playerB.Skull.Should().Be(Skull.None, "Player B should return with no skull after re-login");
        playerB.IsDead.Should().BeFalse("Player B should be alive after respawn");
        playerA.Skull.Should().Be(Skull.White, "Player A should still have white skull from the kill");

        // Act - Step 3: Player B attacks Player A after logging in
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 3 - Yellow skull visibility
        // Player B should have a yellow skull, but only visible to Player A (the victim with white skull)
        playerB.Skull.Should().Be(Skull.None, "Player B should have no permanent skull (base skull is None)");
        
        // Yellow skull should ONLY be visible to Player A (the one being attacked)
        playerB.GetSkull(observer: playerA).Should().Be(Skull.Yellow, "Player A should see Player B with yellow skull (defender's perspective)");
        playerB.PlayerSkull.IsYellowSkull(observer: playerA).Should().BeTrue("Player B should have yellow skull tracked for Player A");
        
        // Yellow skull should NOT be visible to Player B themselves or other players
        playerB.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see themselves with no skull");
        playerB.GetSkull(observer: playerC).Should().Be(Skull.None, "Player C (neutral observer) should see Player B with no skull");
        
        // Player A should still have a white skull visible to everyone
        playerA.Skull.Should().Be(Skull.White, "Player A should remain with white skull");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should see Player A with white skull");
        playerA.GetSkull(observer: playerC).Should().Be(Skull.White, "Player C should see Player A with white skull");
        
        // Verify combat conditions
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should be PZ locked");
        playerB.IsProtectionZoneBlocked.Should().BeTrue("Player B should be PZ locked after attacking");
        
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Third_player_gets_yellow_skull_visible_only_to_victim_when_attacking_white_skull_player()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);

        // Player C starts offline (simulated by not being on the map initially)
        var playerC = PlayerTestDataBuilder.Build(id: 3, name: "PlayerC", level: 10, experience: 1000, vocationType: 1);
        playerC.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        // Act - Step 1: Player A attacks Player B (both unflagged)
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 1
        playerA.Skull.Should().Be(Skull.White, "Player A should receive white skull when attacking unmarked Player B");
        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull after being attacked");

        // Act - Step 2: Player C logs in
        (map[101, 100, 7] as DynamicTile)?.AddCreature(playerC);
        playerC.Login();

        // Assert after Step 2
        playerC.Skull.Should().Be(Skull.None, "Player C should appear with no skull after login");
        playerC.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player C with no skull");
        playerC.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see Player C with no skull");
        
        playerA.Skull.Should().Be(Skull.White, "Player A should still have white skull after Player C logs in");
        playerA.GetSkull(observer: playerC).Should().Be(Skull.White, "Player C should see Player A with white skull");

        // Act - Step 3: Player C attacks Player A (who is skulled)
        playerAttackService.Attack(playerC, playerA);

        // Assert after Step 3 - Key assertions for yellow skull visibility
        // Player C should be flagged with yellow skull, but only Player A can see it
        playerC.Skull.Should().Be(Skull.None, "Player C should have no permanent skull (base skull is None)");
        
        playerC.GetSkull(observer: playerA).Should().Be(Skull.Yellow, "Player A (victim) should see Player C with yellow skull");
        playerC.PlayerSkull.IsYellowSkull(observer: playerA).Should().BeTrue("Player C should have yellow skull tracked for Player A");
        
        // Yellow skull should NOT be visible to Player B or other players
        playerC.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should NOT see yellow skull on Player C");
        playerC.GetSkull(observer: playerC).Should().Be(Skull.None, "Player C should see themselves with no skull");
        playerC.PlayerSkull.IsYellowSkull(observer: playerB).Should().BeFalse("Player C should not have yellow skull for Player B");
        
        // Player A should keep the white skull
        playerA.Skull.Should().Be(Skull.White, "Player A should keep white skull after being attacked by Player C");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should still see Player A with white skull");
        playerA.GetSkull(observer: playerC).Should().Be(Skull.White, "Player C should see Player A with white skull");
        
        // Player B should remain without any skull
        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull throughout the scenario");
        
        // Verify PZ lock and combat conditions
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should be PZ locked");
        playerC.IsProtectionZoneBlocked.Should().BeTrue("Player C should be PZ locked after attacking");
        
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        playerC.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player C should have LogoutBlock condition");
    }
}