using System.Collections.Immutable;
using NeoServer.Domain.Common.Contracts.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Creatures.Monsters;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Monster.Combat;

namespace NeoServer.Domain.Creatures.Monster;

public sealed class MonsterType : IMonsterType
{
    public ushort ManaCost { get; set; }
    public CombatStrategy CombatStrategy { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public Race Race { get; set; }
    public uint Experience { get; set; }
    public ushort Speed { get; set; }
    public uint Health { get; set; }
    public uint MaxHealth { get; set; }
    public IDictionary<LookType, ushort> Look { get; set; }
    public IIntervalChance TargetChance { get; set; }

    public IDictionary<CreatureFlagAttribute, ushort> Flags { get; set; } =
        new Dictionary<CreatureFlagAttribute, ushort>();

    public bool HasFlag(CreatureFlagAttribute flag)
    {
        return Flags.TryGetValue(flag, out var value) && value > 0;
    }

    public MonsterCombatType[] Attacks { get; set; }
    public Dictionary<string, MonsterCombatType> Spells { get; set; }
    public ushort Armor { get; set; }
    public ushort Defense { get; set; }
    public ICombatDefense[] Defenses { get; set; }
    public IIntervalChance VoiceConfig { get; set; }
    public Voice[] Voices { get; set; }
    public ImmutableDictionary<DamageType, sbyte> ElementResistance { get; set; }
    public ushort Immunities { get; set; }
    public Loot.Loot Loot { get; set; }
    public byte MaxSummons { get; set; }
    public IMonsterSummon[] Summons { get; set; }
    public bool HasDistanceAttack { get; set; }
    public byte MaxRangeDistanceAttack { get; set; }
}