using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterStateTest
{
    [Fact]
    public void Monster_turns_state_to_sleeping_when_has_no_target()
    {
        //arrange
        var monster = MonsterTestDataBuilder.Build();

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.Sleeping);
    }

    [Fact]
    public void Monster_turns_state_to_looking_for_enemy_when_has_target_but_unreachable()
    {
        //arrange
        var monsterTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var playerTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 6));

        var monster = MonsterTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build();

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);
        monster.SetAsEnemy(player);

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.LookingForEnemy);
    }

    [Fact]
    public void Monster_turns_state_to_in_combat_when_has_reachable_target()
    {
        //arrange
        var monsterTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var playerTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 7));

        var monster = MonsterTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build();
        
        var map = MapTestDataBuilder.Build(monsterTile, playerTile);

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);
        monster.SetAsEnemy(player);
        
        new TargetDetectorService(map).Update(monster as Domain.Creatures.Monster.Monster);

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
    }

    [Fact]
    public void Monster_turns_state_to_escaping_when_weak()
    {
        //arrange
        var monsterTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var playerTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 7));

        var monster = MonsterTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build();

        monster.Metadata.Flags.Add(CreatureFlagAttribute.RunOnHealth, 100);

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);
        monster.SetAsEnemy(player);

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.Escaping);
    }

    [Fact]
    public void Monster_turns_state_to_looking_for_enemy_when_the_only_target_dies()
    {
        //arrange
        var monsterTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var monster2Tile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 101, 7));
        var playerTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 7));

        var monster = MonsterTestDataBuilder.Build();
        var monster2 = MonsterTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build(hp: 100);

        monsterTile.AddCreature(monster);
        monster2Tile.AddCreature(monster2);
        playerTile.AddCreature(player);

        monster.SetAsEnemy(player);
        monster.UpdateState();

        player.TakeDamage(monster2, new CombatDamage(200, DamageType.Melee));

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.LookingForEnemy);
    }

    [Fact]
    public void Monster_turns_state_to_in_combat_when_cannot_reach_but_has_sight_clear_and_can_attack_from_distance()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);
        var monsterTile = (DynamicTile)map[new Location(100, 100, 7)];
        var playerTile = (DynamicTile)map[new Location(105, 100, 7)];

        var monster = MonsterTestDataBuilder.Build(map: map);
        var player = PlayerTestDataBuilder.Build(hp: 100);

        monster.Metadata.Attacks =
        [
            new MonsterCombatType
            {
                AttackChance = 100
            }
        ];
        
        monster.Metadata.MaxRangeDistanceAttack = 6;

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);

        monster.SetAsEnemy(player);

        new TargetDetectorService(map).Update(monster as Domain.Creatures.Monster.Monster);

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
    }
}