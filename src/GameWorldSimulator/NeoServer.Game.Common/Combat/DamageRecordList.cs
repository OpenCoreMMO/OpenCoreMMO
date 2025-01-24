using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Common.Combat;

public class DamageRecordList
{
    private Dictionary<uint, DamageRecord> DamageRecords { get; } = new();

    public bool HasAnyUnjustifiedDamage { get; private set; }
    public DamageRecord[] All => DamageRecords.Values.ToArray();

    public int TotalDamage
    {
        get
        {
            var damage = 0;
            foreach (var damageRecord in All)
            {
                damage += damageRecord.Damage;
            }

            return damage;
        }
    }

    public ImmutableHashSet<ICreature> AllDamageParticipants
    {
        get
        {
            var participants = new HashSet<ICreature>();
            foreach (var damageRecord in All)
            {
                if (damageRecord.Aggressor is not ICreature creature) continue;
                participants.Add(creature);
            }

            return participants.ToImmutableHashSet();
        }
    }

    public DamageRecord GetCreatureDamage(ICreature creature) => DamageRecords.GetValueOrDefault(creature.CreatureId);

    public void AddOrUpdateDamage(IThing thing, ushort damage, bool unjustified)
    {
        if (Guard.IsNull(thing))
            throw new ArgumentNullException(nameof(thing), $"[{nameof(DamageRecordList)}] Creature cannot be null.");

        if (damage == 0) return;

        uint key = 0;

        if (thing is ICreature creature) key = creature.CreatureId;
        if (thing is IItem item) key = item.ServerId;

        HasAnyUnjustifiedDamage = unjustified || HasAnyUnjustifiedDamage;

        if (DamageRecords.TryGetValue(key, out var record))
        {
            record.AddDamage(damage, unjustified);
            return;
        }

        DamageRecords.Add(key, new DamageRecord(thing, damage, unjustified));
    }

    public DamageRecordResult GetDamageRecords(DeathConfiguration deathConfiguration)
    {
        if (!deathConfiguration.IsDeathListEnabled)
            return new DamageRecordResult(DamageRecords.Values.ToList(), HasAnyUnjustifiedDamage);

        if (deathConfiguration.MaxDeathRecords <= 0) return new DamageRecordResult([], false);

        var damageList = new List<DamageRecord>();

        var records = DamageRecords.Values.ToArray();
        Array.Sort(records, (a, b) => b.LastDamageTime.CompareTo(a.LastDamageTime));

        var count = Math.Min(deathConfiguration.MaxDeathRecords, records.Length);

        var hasAnyUnjustifiedDamage = false;

        foreach (var record in records)
        {
            if (count >= deathConfiguration.MaxDeathRecords) break;
            if (record.NumberOfHits < deathConfiguration.DeathAssistCount) continue;
            if (record.LastDamageTime >= DateTime.Now.Ticks - deathConfiguration.DeathListRequiredTime) continue;
            
            hasAnyUnjustifiedDamage = record.Unjustified || hasAnyUnjustifiedDamage;

            damageList.Add(record);
            count++;
        }

        return new DamageRecordResult(damageList, hasAnyUnjustifiedDamage);
    }

    public void Clear()
    {
        DamageRecords.Clear();
    }
}