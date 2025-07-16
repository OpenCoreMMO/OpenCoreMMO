using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterTest
{
    [Fact]
    public void Monster_is_not_injured_when_attacked_by_another_monster()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var sumonstert = MonsterTestDataBuilder.Build();
        var attacker = MonsterTestDataBuilder.Build();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sumonstert);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(attacker);

        var monsterCombatService = MonsterCombatServiceTestBuilder.Build(map);

        //act
        monsterCombatService.Attack(attacker, sumonstert);

        //assert
        sumonstert.HealthPoints.Should().Be(sumonstert.MaxHealthPoints);
    }

    [Fact]
    public void Monster_is_not_injured_when_attacked_by_a_summon_of_a_monster()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var monster = MonsterTestDataBuilder.Build();

        var master = MonsterTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(monster);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(summon);

        var monsterCombatService = MonsterCombatServiceTestBuilder.Build(map);

        //act
        monsterCombatService.Attack(summon, monster);

        //assert
        monster.HealthPoints.Should().Be(monster.MaxHealthPoints);
    }

    [Fact]
    public void Monster_is_injured_when_attacked_by_a_summon_of_a_player()
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
        monsterCombatService.Attack(summon, monster);

        //assert
        monster.HealthPoints.Should().BeLessThan(monster.MaxHealthPoints);
    }

    [Fact]
    public void Player_summon_is_injured_when_attacked_by_a_monster()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var monster = MonsterTestDataBuilder.Build(9000, isHostile: true);

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