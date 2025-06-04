using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterTest
{
    [Fact]
    public void Monster_is_not_injured_when_attacked_by_another_monster()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var sut = MonsterTestDataBuilder.Build();
        var attacker = MonsterTestDataBuilder.Build();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(attacker);


        //act
        attacker.Attack(sut);

        //assert
        sut.HealthPoints.Should().Be(sut.MaxHealthPoints);
    }

    [Fact]
    public void Monster_is_not_injured_when_attacked_by_a_summon_of_a_monster()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var sut = MonsterTestDataBuilder.Build();

        var master = MonsterTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(summon);

        //act
        summon.Attack(sut);

        //assert
        sut.HealthPoints.Should().Be(sut.MaxHealthPoints);
    }

    [Fact(Skip = "TODO: Fix later")]
    [Trait("Category", "FixLater")]
    public void Monster_is_injured_when_attacked_by_a_summon_of_a_player()
    {
        //arrange

        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var sut = MonsterTestDataBuilder.Build(9000);

        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master, 4000, 5000);

        (map[100, 100, 7] as DynamicTile)?.AddCreature(sut);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 101, 7] as DynamicTile)?.AddCreature(summon);

        //act
        summon.Attack(sut);

        //assert
        sut.HealthPoints.Should().BeLessThan(sut.MaxHealthPoints);
    }
}