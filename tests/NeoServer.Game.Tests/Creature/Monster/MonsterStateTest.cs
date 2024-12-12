using FluentAssertions;
using NeoServer.Game.Combat.Attacks.Legacy;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Models.Tiles;
using Xunit;

namespace NeoServer.Game.Tests.Creature.Monster;

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

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);
        monster.SetAsEnemy(player);

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

        player.ReceiveAttackFrom(monster2, new CombatDamageList([new CombatDamage(200, DamageType.Melee)]));

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

        monster.Metadata.Attacks = new IMonsterCombatAttack[]
        {
            new MonsterCombatAttack
            {
                Chance = 100,
                CombatAttack = new DistanceCombatAttack(6, ShootType.Arrow)
            }
        };
        monster.Metadata.MaxRangeDistanceAttack = 6;

        monsterTile.AddCreature(monster);
        playerTile.AddCreature(player);

        monster.SetAsEnemy(player);

        //act
        monster.UpdateState();

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
    }
}