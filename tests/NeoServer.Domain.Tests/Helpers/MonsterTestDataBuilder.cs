using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Models.Spawns;
using NeoServer.Domain.World.Services;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.Tests.Helpers;

public static class MonsterTestDataBuilder
{
    public static IMonster Build(uint maxHealth = 100, ushort speed = 200, IMap map = null, bool isHostile = true,
        Dictionary<CreatureFlagAttribute, ushort>? flags = null, string name = null)
    {
        map ??= MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

        var mapTool = new MapTool(map, pathFinder);

        var monsterType = new MonsterType
        {
            Name = name ?? "Monster X",
            MaxHealth = maxHealth,
            Speed = speed,
            TargetChance = new IntervalChance(1000, 50),
            Attacks =
            [
                new MonsterCombatType
                {
                    Interval = 0,
                    AttackChance = 100,
                    CombatParameter = new CombatParameter
                    {
                        MinDamage = 10,
                        MaxDamage = 100,
                        DamageType = DamageType.Melee
                    }
                }
            ],
            Flags =
            {
                [CreatureFlagAttribute.Hostile] = (ushort)(isHostile ? 1 : 0)
            }
        };

        if (flags != null)
            foreach (var flag in flags)
                monsterType.Flags[flag.Key] = flag.Value;

        return new Monster(monsterType, mapTool, spawnPoint);
    }

    public static IMonster BuildSummon(ICreature master, ushort minDamage = 10, ushort maxDamage = 100,
        byte targetDistance = 1)
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);

        var mapTool = new MapTool(map, pathFinder);

        var monsterType = new MonsterType
        {
            Name = "Monster X",
            MaxHealth = 100,
            TargetChance = new IntervalChance(1000, 50),
            Attacks =
            [
                new MonsterCombatType
                {
                    Interval = 0,
                    AttackChance = 100,
                    CombatParameter = new CombatParameter
                    {
                        MinDamage = minDamage,
                        MaxDamage = maxDamage,
                        DamageType = DamageType.Melee
                    }
                }
            ]
        };

        monsterType.Flags.Add(CreatureFlagAttribute.Hostile, 1);
        monsterType.Flags.Add(CreatureFlagAttribute.TargetDistance, targetDistance);

        return new Summon(monsterType, mapTool, master);
    }
}