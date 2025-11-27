using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Combat.Player;
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

    [Fact]
    [Trait("Category", "PvP")]
    public void Players_do_not_get_skull_or_pz_block_when_fighting_in_pvp_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        // Create tiles with PvpZone flag set
        var tileA = map[100, 100, 7] as DynamicTile;
        var tileB = map[100, 101, 7] as DynamicTile;

        // Use reflection to set the PvpZone flag on both tiles
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(tileA, (uint)TileFlags.PvpZone);
        flagsField.SetValue(tileB, (uint)TileFlags.PvpZone);

        // Add players to PvP zone tiles
        tileA?.AddCreature(playerA);
        tileB?.AddCreature(playerB);

        // Verify tiles are PvP zones
        tileA.PvpZone.Should().BeTrue("Tile A should be a PvP zone");
        tileB.PvpZone.Should().BeTrue("Tile B should be a PvP zone");

        // Act - Step 1: Player A attacks Player B in PvP zone
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 1
        playerA.Skull.Should().Be(Skull.None, "Player A should NOT receive any skull when attacking in PvP zone");
        playerA.IsProtectionZoneBlocked.Should().BeFalse("Player A should NOT get PZ block when attacking in PvP zone");
        
        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull");
        
        // Act - Step 2: Player B attacks Player A in retaliation
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 2
        playerB.Skull.Should().Be(Skull.None, "Player B should NOT receive any skull when attacking in PvP zone");
        playerB.IsProtectionZoneBlocked.Should().BeFalse("Player B should NOT get PZ block when attacking in PvP zone");
        
        playerA.Skull.Should().Be(Skull.None, "Player A should remain without skull throughout the PvP zone combat");
        
        // Verify skull visibility - no skulls should be visible to any observer
        playerA.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see Player A with no skull");
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        
        playerA.PlayerSkull.IsYellowSkull(observer: playerB).Should().BeFalse("Player A should not have yellow skull for Player B");
        playerB.PlayerSkull.IsYellowSkull(observer: playerA).Should().BeFalse("Player B should not have yellow skull for Player A");
        
        // Combat conditions (LogoutBlock) may still apply, but PZ block should not
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition (combat state)");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition (combat state)");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_in_pvp_zone_gets_skull_and_pz_block_when_attacking_player_outside_pvp_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        // Create tiles - Player A in PvP zone, Player B in normal zone
        var tileA = map[100, 100, 7] as DynamicTile;
        var tileB = map[100, 101, 7] as DynamicTile;

        // Use reflection to set the PvpZone flag only on tile A
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(tileA, (uint)TileFlags.PvpZone);

        // Add players to their respective tiles
        tileA?.AddCreature(playerA);
        tileB?.AddCreature(playerB);

        // Verify tile zones
        tileA.PvpZone.Should().BeTrue("Tile A should be a PvP zone");
        tileB.PvpZone.Should().BeFalse("Tile B should NOT be a PvP zone");

        // Act - Step 1: Player A (in PvP zone) attacks Player B (not in PvP zone)
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 1
        playerA.Skull.Should().Be(Skull.White, "Player A should receive white skull when attacking player outside PvP zone");
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should get PZ block when attacking player outside PvP zone");
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        
        playerB.Skull.Should().Be(Skull.None, "Player B should remain without skull");
        
        // Verify skull visibility
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should see Player A with white skull");
        playerA.PlayerSkull.IsYellowSkull(observer: playerB).Should().BeFalse("Player A should not have yellow skull for Player B");
        
        // Act - Step 2: Player B attacks Player A back
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 2
        playerB.Skull.Should().Be(Skull.None, "Player B should NOT receive skull when retaliating against white skull attacker");
        playerB.IsProtectionZoneBlocked.Should().BeFalse("Player B should not get PZ block after attacking");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition");
        
        // Player A should still have white skull
        playerA.Skull.Should().Be(Skull.White, "Player A should keep white skull after being attacked");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should still see Player A with white skull");
        
        // Verify Player B has no skull from any observer's perspective
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        playerB.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see themselves with no skull");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_outside_pvp_zone_does_not_get_skull_or_pz_block_when_attacking_player_inside_pvp_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1);
        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);

        // Create tiles - Player A in PvP zone, Player B in normal zone
        var tileA = map[100, 100, 7] as DynamicTile;
        var tileB = map[100, 101, 7] as DynamicTile;

        // Use reflection to set the PvpZone flag only on tile A
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(tileA, (uint)TileFlags.PvpZone);

        // Add players to their respective tiles
        tileA?.AddCreature(playerA);
        tileB?.AddCreature(playerB);

        // Verify tile zones
        tileA.PvpZone.Should().BeTrue("Tile A should be a PvP zone");
        tileB.PvpZone.Should().BeFalse("Tile B should NOT be a PvP zone");

        // Act - Step 1: Player B (not in PvP zone) attacks Player A (in PvP zone)
        playerAttackService.Attack(playerB, playerA);

        // Assert after Step 1
        playerB.Skull.Should().Be(Skull.None, "Player B should NOT receive white skull when attacking player in PvP zone");
        playerB.IsProtectionZoneBlocked.Should().BeFalse("Player B should NOT get PZ block when attacking player in PvP zone");
        playerB.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player B should have LogoutBlock condition (combat state)");
        
        playerA.Skull.Should().Be(Skull.None, "Player A should remain without skull");
        
        // Verify skull visibility - no skulls yet
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        playerA.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see Player A with no skull");
        
        // Act - Step 2: Player A (in PvP zone) attacks Player B (not in PvP zone) back
        playerAttackService.Attack(playerA, playerB);

        // Assert after Step 2
        playerA.Skull.Should().Be(Skull.White, "Player A should receive white skull when attacking player outside PvP zone");
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should get PZ block when attacking player outside PvP zone");
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
        
        // Player B should still have no skull
        playerB.Skull.Should().Be(Skull.None, "Player B should still have no skull");
        playerB.GetSkull(observer: playerA).Should().Be(Skull.None, "Player A should see Player B with no skull");
        playerB.GetSkull(observer: playerB).Should().Be(Skull.None, "Player B should see themselves with no skull");
        
        // Verify Player A's skull visibility
        playerA.GetSkull(observer: playerB).Should().Be(Skull.White, "Player B should see Player A with white skull");
        playerA.GetSkull(observer: playerA).Should().Be(Skull.White, "Player A should see themselves with white skull");
        
        // Verify no yellow skull tracking
        playerA.PlayerSkull.IsYellowSkull(observer: playerB).Should().BeFalse("Player A should not have yellow skull for Player B");
        playerB.PlayerSkull.IsYellowSkull(observer: playerA).Should().BeFalse("Player B should not have yellow skull for Player A");
    }

    [Fact]
    [Trait("Category", "PvP")]
    public void Player_receives_red_skull_after_reaching_unjustified_kills_threshold()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerAttackService = AttackServiceTestBuilder.BuildPlayerCombatService(map);

        var playerA = PlayerTestDataBuilder.Build(id: 1, name: "PlayerA", level: 10, experience: 1000, vocationType: 1, hp: 100);
        
        // Boost Player A's skills to ensure they can kill
        for (var i = 1; i <= 100; i++)
        {
            playerA.IncreaseSkillCounter(SkillType.Fist, long.MaxValue);
        }

        playerA.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(playerA);

        // Precondition: Player A has 2 unjustified kills already
        playerA.SetNumberOfKills(killsInLastDay: 2, killsInLastWeek: 2, killsInLastMonth: 2);

        // Verify Player A starts with no skull or white skull (if they already have kills, they might have white)
        var initialSkull = playerA.Skull;
        (initialSkull == Skull.None || initialSkull == Skull.White).Should().BeTrue("Player A should start with no skull or white skull");

        // Create Player B as the 3rd victim
        var playerB = PlayerTestDataBuilder.Build(id: 2, name: "PlayerB", level: 10, experience: 1000, vocationType: 1, hp: 1);
        playerB.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(playerB);

        // Act - Player A attacks and kills Player B (3rd unjustified kill)
        playerAttackService.Attack(playerA, playerB);
        
        // Kill Player B
        while (playerB.HealthPoints > 0)
        {
            playerAttackService.Attack(playerA, playerB);
        }

        // Assert - Player B is dead
        playerB.IsDead.Should().BeTrue("Player B should be dead after the attack");

        // Increment kill count to simulate the 3rd kill
        playerA.SetNumberOfKills(killsInLastDay: 3, killsInLastWeek: 3, killsInLastMonth: 3);

        // Trigger skull update based on kill count (this would normally be done by the game system)
        // We need to create a new PlayerSkullService with the game configuration
        var gameConfiguration = new GameConfiguration
        {
            PvP = new PvPConfiguration(PvpType.OpenPvP, ProtectionLevel: 2)
            {
                DayKillsToRedSkull = 3,
                WeekKillsToRedSkull = 5,
                MonthKillsToRedSkull = 10,
                DayKillsToBlackSkull = 6,
                WeekKillsToBlackSkull = 10,
                MonthKillsToBlackSkull = 20,
                SkullSystemEnabled = true
            }
        };
        var playerSkullService = new PlayerSkullService(gameConfiguration);
        playerSkullService.UpdatePlayerSkull(playerA);

        // Assert - Player A should now have red skull after 3rd kill
        playerA.Skull.Should().Be(Skull.Red, "Player A should receive red skull after 3rd unjustified kill");

        // Verify skull visibility
        playerA.GetSkull(observer: playerB).Should().Be(Skull.Red, "Player B should see Player A with red skull");
        playerA.GetSkull(observer: playerA).Should().Be(Skull.Red, "Player A should see themselves with red skull");

        // Verify combat conditions
        playerA.IsProtectionZoneBlocked.Should().BeTrue("Player A should be PZ locked");
        playerA.HasCondition(ConditionType.LogoutBlock).Should().BeTrue("Player A should have LogoutBlock condition");
    }
}