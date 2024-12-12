using NeoServer.Game.Combat.Attacks.Legacy;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creature.Monster;
using NeoServer.Game.Creature.Monster.Summon;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Game.World.Services;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Game.Tests.Helpers;

public static class MonsterTestDataBuilder
{
    public static IMonster Build(uint maxHealth = 100, ushort speed = 200, IMap map = null)
    {
        map ??= MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

        var mapTool = new MapTool(map, pathFinder);

        var monsterType = new MonsterType
        {
            Name = "Monster X",
            MaxHealth = maxHealth,
            Speed = speed,
            Attacks = new IMonsterCombatAttack[]
            {
                new MonsterCombatAttack
                {
                    MinDamage = 10,
                    MaxDamage = 100,
                    Interval = 0,
                    DamageType = DamageType.Melee,
                    Chance = 100,
                    CombatAttack = new MeleeCombatAttack
                    {
                        Min = 10,
                        Max = 100
                    }
                }
            }
        };
        return new Monster(monsterType, mapTool, spawnPoint);
    }

    public static IMonster BuildSummon(ICreature master, ushort minDamage = 10, ushort maxDamage = 100)
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);

        var mapTool = new MapTool(map, pathFinder);

        var monsterType = new MonsterType
        {
            Name = "Monster X",
            MaxHealth = 100,
            Attacks = new IMonsterCombatAttack[]
            {
                new MonsterCombatAttack
                {
                    MinDamage = minDamage,
                    MaxDamage = maxDamage,
                    Interval = 0,
                    DamageType = DamageType.Melee,
                    Chance = 100,
                    CombatAttack = new MeleeCombatAttack
                    {
                        Min = minDamage,
                        Max = maxDamage
                    }
                }
            }
        };

        monsterType.Flags.Add(CreatureFlagAttribute.Hostile, 1);

        return new Summon(monsterType, mapTool, master);
    }
}