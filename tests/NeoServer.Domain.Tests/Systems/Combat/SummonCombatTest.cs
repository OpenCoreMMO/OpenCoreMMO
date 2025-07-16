using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Systems.Combat;

public class SummonCombatTest
{
    [Fact]
    public void Player_summon_is_injured_when_attacked_by_a_monster()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var monster = MonsterTestDataBuilder.Build(9000);

        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master, 4000, 5000);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(monster);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(summon);

        var monsterCombatService = MonsterCombatServiceTestBuilder.Build(map);

        //act
        monsterCombatService.Attack(monster, summon);

        //assert
        summon.HealthPoints.Should().BeLessThan(summon.MaxHealthPoints);
    }
}