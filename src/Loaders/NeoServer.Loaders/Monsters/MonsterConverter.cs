using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Loaders.Monsters.Converters;
using Serilog;

namespace NeoServer.Loaders.Monsters;

public static class MonsterConverter
{
    public static IMonsterType Convert(MonsterData monsterData, GameConfiguration configuration,
        IMonsterDataManager monsters, ILogger logger, IItemTypeStore itemTypeStore)
    {
        var monster = new MonsterType
        {
            Name = monsterData.Name,
            MaxHealth = monsterData.Health.Max,
            Look = new Dictionary<LookType, ushort>
            {
                { LookType.Type, monsterData.Look.Type }, { LookType.Corpse, monsterData.Look.Corpse },
                { LookType.Body, monsterData.Look.Body }, { LookType.Legs, monsterData.Look.Legs },
                { LookType.Head, monsterData.Look.Head },
                { LookType.Feet, monsterData.Look.Feet }, { LookType.Addon, monsterData.Look.Addons }
            },
            Speed = monsterData.Speed,
            Armor = ushort.Parse(monsterData.Defense.Armor),
            Defense = ushort.Parse(monsterData.Defense.Defense),
            Experience = (uint)(monsterData.Experience * configuration.ExperienceRate),
            Race = ParseRace(monsterData.Race),
            TargetChance = new IntervalChance(System.Convert.ToUInt16(monsterData.Targetchange.Interval),
                System.Convert.ToByte(monsterData.Targetchange.Chance))
        };

        if (monsterData.Voices != null)
        {
            monster.VoiceConfig = new IntervalChance(System.Convert.ToUInt16(monsterData.Voices.Interval),
                System.Convert.ToByte(monsterData.Voices.Chance));
            monster.Voices = monsterData.Voices.Sentences
                .Select(x =>
                    new Voice(x.Sentence, x.Yell ? SpeechType.MonsterYell : SpeechType.MonsterSay)).ToArray();
        }

        monster.Attacks = MonsterAttackConverter.Convert(monsterData, logger)?.OrderByDescending(x => x.AttackChance)
            .ToArray();

        monster.HasDistanceAttack = monster.Attacks.Any(x => x.CombatParameter.Range > 0);

        monster.MaxRangeDistanceAttack = monster.Attacks.Max(x => x.CombatParameter.Range) ?? 0;

        monster.ElementResistance = MonsterResistanceConverter.Convert(monsterData).ToImmutableDictionary();
        monster.Immunities = MonsterImmunityConverter.Convert(monsterData);

        monster.Defenses = MonsterDefenseConverter.Convert(monsterData, monsters);

        monster.Loot = MonsterLootConverter.Convert(monsterData, itemTypeStore);

        var summons = MonsterSummonConverter.Convert(monsterData);
        monster.MaxSummons = (byte)summons.Item1;
        monster.Summons = summons.Item2;

        foreach (var flag in monsterData.Flags)
        {
            var creatureFlag = ParseCreatureFlag(flag.Key);
            monster.Flags.Add(creatureFlag, flag.Value);
        }

        return monster;
    }

    private static CreatureFlagAttribute ParseCreatureFlag(string flag)
    {
        return flag.ToLower() switch
        {
            "summonable" => CreatureFlagAttribute.Summonable,
            "attackable" => CreatureFlagAttribute.Attackable,
            "hostile" => CreatureFlagAttribute.Hostile,
            "illusionable" => CreatureFlagAttribute.Illusionable,
            "convinceable" => CreatureFlagAttribute.Convinceable,
            "pushable" => CreatureFlagAttribute.Pushable,
            "canpushitems" => CreatureFlagAttribute.CanPushItems,
            "canpushcreatures" => CreatureFlagAttribute.CanPushCreatures,
            "targetdistance" => CreatureFlagAttribute.TargetDistance,
            "staticattack" => CreatureFlagAttribute.StaticAttack,
            "runonhealth" => CreatureFlagAttribute.RunOnHealth,
            "isboss" => CreatureFlagAttribute.IsBoss,
            "rewardboss" => CreatureFlagAttribute.RewardBoss,
            "lightcolor" => CreatureFlagAttribute.LightColor,
            _ => CreatureFlagAttribute.None
        };
    }

    private static Race ParseRace(string race)
    {
        return race switch
        {
            "venom" => Race.Venom,
            "blood" => Race.Bood,
            "undead" => Race.Undead,
            "fire" => Race.Fire,
            "energy" => Race.Energy,
            _ => Race.Bood
        };
    }
}