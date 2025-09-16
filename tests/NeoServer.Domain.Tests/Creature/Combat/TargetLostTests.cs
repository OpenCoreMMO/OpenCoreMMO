using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Combat;

public class TargetLostTests
{
    [Fact]
    public void Player_loses_target_when_target_moves_out_of_sight()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = MonsterTestDataBuilder.Build(map: map);

        // Set monster as player's target
        player.SetAttackTarget(monster);

        //act
        // Move monster far away (out of sight range)
        monster.SetNewLocation(new Location(120, 120, 7));

        // Trigger OnSpectatorMoved to check for target loss
        player.OnSpectatorMoved(monster);

        //assert
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_loses_target_when_target_goes_to_another_floor()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = MonsterTestDataBuilder.Build(map: map);

        // Set monster as player's target
        player.SetAttackTarget(monster);

        //act
        // Move monster to different floor
        monster.SetNewLocation(new Location(105, 105, 8));

        // Trigger OnSpectatorMoved to check for target loss
        player.OnSpectatorMoved(monster);

        //assert
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_loses_target_when_target_becomes_invisible()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = MonsterTestDataBuilder.Build(map: map);

        // Set monster as player's target
        player.SetAttackTarget(monster);

        //act
        // Make monster invisible
        monster.TurnInvisible();
        player.Think(1);

        //assert
        // This test validates the current behavior
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_loses_target_when_target_dies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = (NeoServer.Domain.Creatures.Monster.Monster)MonsterTestDataBuilder.Build(map: map);

        // Set monster as player's target
        player.SetAttackTarget(monster);

        //act
        // Kill the monster
        monster.Death(player);

        // Trigger OnSpectatorMoved to check for target loss
        player.Think(1);

        //assert
        // The target reference remains until explicitly cleared
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_loses_target_when_target_logs_out()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var targetPlayer = PlayerTestDataBuilder.Build(id: 2, name: "TargetPlayer");
        targetPlayer.SetNewLocation(new Location(105, 106, 7));

        // Set target player as player's target
        player.SetAttackTarget(targetPlayer);

        //act
        // Target player logs out
        targetPlayer.Logout(forced: true);

        player.Think(1);

        //assert
        // The target reference remains until explicitly cleared
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_stops_attack_when_HandleTargetLost_is_called()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = (Player)PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = MonsterTestDataBuilder.Build(map: map);

        // Set monster as player's target and start attacking
        player.SetAttackTarget(monster);
        player.Attacking.Should().BeTrue();

        //act
        // Simulate target being lost
        player.HandleTargetLost();

        //assert
        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Monster_removes_target_from_list_when_target_moves_out_of_sight()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map) as Domain.Creatures.Monster.Monster;

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        // Add player as target
        monster.Targets.AddTarget(player);

        //act
        // Move player out of sight
        player.SetNewLocation(new Location(120, 120, 7));

        // Trigger the movement event by calling OnSpectatorMoved on the monster
        monster.OnSpectatorMoved(player);

        //assert
        monster.Targets.HasTarget(player).Should().BeFalse();
        monster.Targets.Any().Should().BeFalse();
    }

    [Fact]
    public void Monster_removes_target_from_list_when_target_dies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map) as Domain.Creatures.Monster.Monster;
        
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        // Add player as target
        monster.Targets.AddTarget(player);

        //act
        // Kill the player
        ((Player)player).Death(monster);

        monster.OnSpectatorDies(player);

        //assert
        monster.Targets.HasTarget(player).Should().BeFalse();
        monster.Targets.Any().Should().BeFalse();
    }

    [Fact]
    public void Monster_removes_target_from_list_when_player_logs_out()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map) as Domain.Creatures.Monster.Monster;
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        // Add player as target
        monster.Targets.AddTarget(player);

        //act
        // Player logs out
        player.Logout();

        // Trigger the logout event by calling OnSpectatorMoved (this should trigger the cleanup)
        monster.OnSpectatorLoggedOut(player);

        //assert
        monster.Targets.HasTarget(player).Should().BeFalse();
        monster.Targets.Any().Should().BeFalse();
    }

    [Fact]
    public void Player_is_not_target_lost_when_no_target_exists()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        //act & assert
        player.IsTargetLost().Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_is_not_target_lost_when_target_is_in_sight()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));
        var monster = MonsterTestDataBuilder.Build(map: map);
        monster.SetNewLocation(new Location(105, 106, 7)); // Place monster adjacent to player

        // Set monster as player's target
        player.SetAttackTarget(monster);

        //act & assert
        player.IsTargetLost().Should().BeFalse();
        player.CurrentTarget.Should().Be(monster);
    }
}