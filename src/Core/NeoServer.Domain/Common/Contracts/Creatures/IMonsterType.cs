using System.Collections.Immutable;
using NeoServer.Domain.Common.Contracts.Combat;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;
using NeoServer.Domain.Common.Contracts.Creatures.Monsters;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IMonsterType : ICreatureType
{
    ushort Armor { get; set; }
    ushort Defense { get; set; }

    public uint Experience { get; set; }
    public MonsterCombatType[] Attacks { get; set; }
    public Dictionary<string, MonsterCombatType> Spells { get; set; }
    public ICombatDefense[] Defenses { get; set; }

    IDictionary<CreatureFlagAttribute, ushort> Flags { get; set; }
    IIntervalChance TargetChance { get; set; }

    /// <summary>
    ///     Monster's phases
    /// </summary>
    Voice[] Voices { get; set; }

    /// <summary>
    ///     Voice interval and chance to happen
    /// </summary>
    IIntervalChance VoiceConfig { get; set; }

    ImmutableDictionary<DamageType, sbyte> ElementResistance { get; set; }
    Race Race { get; set; }
    Loot Loot { get; set; }
    IMonsterSummon[] Summons { get; set; }
    byte MaxSummons { get; set; }
    ushort Immunities { get; set; }
    bool HasDistanceAttack { get; set; }
    byte MaxRangeDistanceAttack { get; set; }
    ushort ManaCost { get; set; }
    bool HasFlag(CreatureFlagAttribute flag);
}