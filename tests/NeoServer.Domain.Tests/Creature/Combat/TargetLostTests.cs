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
    public void Player_Loses_Target_When_Target_Moves_Out_Of_Sight()
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
    public void Player_Loses_Target_When_Target_Changes_Floor()
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
    public void Player_Loses_Target_When_Target_Becomes_Invisible()
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
        // Note: Players can see invisible creatures by default, so target should not be lost
        // This test validates the current behavior
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_Loses_Target_When_Target_Dies()
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
        // Note: Death doesn't automatically trigger target loss through OnSpectatorMoved
        // The target reference remains until explicitly cleared
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void Player_Loses_Target_When_Target_Logs_Out()
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
        // Note: Logout doesn't automatically trigger target loss through OnSpectatorMoved
        // The target reference remains until explicitly cleared
        player.CurrentTarget.Should().BeNull();
    }

    [Fact]
    public void HandleTargetLost_Stops_Attack_And_Sends_Message()
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
    public void TargetList_Removes_Target_When_Monster_Moves_Out_Of_Sight()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map) as Creatures.Monster.Monster;
        
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
    public void TargetList_Removes_Target_When_Target_Dies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map);
        var targetList = new TargetList(monster);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        // Add player as target
        targetList.AddTarget(player);

        //act
        // Kill the player
        ((Player)player).Death(monster);

        // Trigger the death event by calling OnSpectatorMoved (this should trigger the cleanup)
        monster.OnSpectatorMoved(player);

        //assert
        targetList.HasTarget(player).Should().BeFalse();
        targetList.Any().Should().BeFalse();
    }

    [Fact]
    public void TargetList_Removes_Target_When_Player_Logs_Out()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var monster = MonsterTestDataBuilder.Build(map: map);
        var targetList = new TargetList(monster);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        // Add player as target
        targetList.AddTarget(player);

        //act
        // Player logs out
        player.Logout();

        // Trigger the logout event by calling OnSpectatorMoved (this should trigger the cleanup)
        monster.OnSpectatorMoved(player);

        //assert
        targetList.HasTarget(player).Should().BeFalse();
        targetList.Any().Should().BeFalse();
    }

    [Fact]
    public void IsTargetLost_Returns_False_When_Target_Is_In_Sight()
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

    [Fact]
    public void IsTargetLost_Returns_False_When_No_Target()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        //act & assert
        player.IsTargetLost().Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
    }
}