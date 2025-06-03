using FluentAssertions;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Monster;

public class MonsterEscapeTests
{
    [Fact]
    public void Monster_stops_attack_and_follow_while_escaping()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Flags.Add(CreatureFlagAttribute.Hostile, 1);

        var enemy = PlayerTestDataBuilder.Build();
        using var monitor = monster.Monitor();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(monster);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(enemy);

        monster.SetAttackTarget(enemy);
        monster.SetAsEnemy(enemy);

        //act
        monster.Escape();

        //assert
        monitor.Should().Raise(nameof(monster.OnStoppedAttack));
        monster.IsFollowing.Should().BeFalse();
    }
}