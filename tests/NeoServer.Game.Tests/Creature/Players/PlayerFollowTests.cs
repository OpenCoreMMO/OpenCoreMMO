using FluentAssertions;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Models.Tiles;
using Xunit;

namespace NeoServer.Game.Tests.Creature.Players;

public class PlayerFollowTests
{
    [Fact]
    [ThreadBlocking]
    public void Player_does_not_follow_dead_creature()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var sut = PlayerTestDataBuilder.Build(hp: 200);
        var enemy = MonsterTestDataBuilder.Build(1);
        using var monitor = sut.Monitor();

        var fpp = new FindPathParams(true, true, true, false, 12, 0, 12, false);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[100, 108, 7] as DynamicTile)?.AddCreature(enemy);

        sut.Follow(enemy, fpp);
        enemy.ReceiveAttackFrom(sut, new CombatDamageList([new CombatDamage(200, DamageType.Melee)]));

        //act
        sut.Follow(enemy, fpp);

        //assert
        sut.IsFollowing.Should().BeFalse();
        sut.Following.Should().BeNull();
        sut.HasNextStep.Should().BeFalse();
        monitor.Should().Raise(nameof(sut.OnStoppedWalking));
    }

    [Fact]
    public void Player_does_not_emit_stopped_follow_event_if_has_no_further_step()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

        var sut = PlayerTestDataBuilder.Build(hp: 200);
        var enemy = MonsterTestDataBuilder.Build(1);
        using var monitor = sut.Monitor();

        var fpp = new FindPathParams(true, true, true, false, 12, 0, 12, false);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(enemy);

        sut.Follow(enemy, fpp);
        enemy.ReceiveAttackFrom(sut, new CombatDamageList([new CombatDamage(200, DamageType.Melee)]));

        //act
        sut.Follow(enemy, fpp);

        //assert
        sut.IsFollowing.Should().BeFalse();
        sut.Following.Should().BeNull();
        sut.HasNextStep.Should().BeFalse();
        monitor.Should().NotRaise(nameof(sut.OnStoppedWalking));
    }

    [Fact]
    [ThreadBlocking]
    public void Player_does_not_follow_if_creature_disappears()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var sut = PlayerTestDataBuilder.Build(hp: 200);
        var enemy = MonsterTestDataBuilder.Build(1000);
        using var monitor = sut.Monitor();

        var fpp = new FindPathParams(true, true, true, false, 12, 0, 12, false);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[100, 109, 7] as DynamicTile)?.AddCreature(enemy);

        sut.Follow(enemy, fpp);
        enemy.ReceiveAttackFrom(sut, new CombatDamageList([new CombatDamage(200, DamageType.Melee)]));

        (map[100, 109, 7] as DynamicTile)?.RemoveCreature(enemy, out _);
        (map[100, 110, 7] as DynamicTile)?.AddCreature(enemy);

        //act
        sut.Follow(enemy, fpp);

        //assert
        sut.IsFollowing.Should().BeFalse();
        sut.Following.Should().BeNull();
        sut.HasNextStep.Should().BeFalse();
        monitor.Should().Raise(nameof(sut.OnStoppedWalking));
    }
}