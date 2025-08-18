using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterEscapeTests
{
    [Fact]
    public void Monster_stops_follow_while_escaping()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

        var monster = MonsterTestDataBuilder.Build();

        var enemy = PlayerTestDataBuilder.Build();
        using var monitor = monster.Monitor();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(monster);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(enemy);

        monster.SetAttackTarget(enemy);
        monster.SetAsEnemy(enemy);

        //act
        monster.Escape();

        //assert
        monster.IsFollowing.Should().BeFalse();
    }
}