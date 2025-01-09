using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Common.Combat;

public class DamageRecordList
{
    private Dictionary<uint, DamageRecord> DamageRecords { get; } = new();

    public void AddOrUpdateDamage(IThing thing, ushort damage)
    {
        if (Guard.IsNull(thing))
            throw new ArgumentNullException(nameof(thing), $"[{nameof(DamageRecordList)}] Creature cannot be null.");

        if (damage == 0) return;

        uint key = 0;

        if (thing is ICreature creature) key = creature.CreatureId;
        if (thing is IItem item) key = item.ServerId;

        if (DamageRecords.TryGetValue(key, out var record))
        {
            record.AddDamage(damage);
            return;
        }

        DamageRecords.Add(key, new DamageRecord(thing, damage));
    }

    public List<DamageRecord> GetDamageRecords(DeathConfiguration deathConfiguration)
    {
        if (!deathConfiguration.IsDeathListEnabled) return DamageRecords.Values.ToList();

        if (deathConfiguration.MaxDeathRecords <= 0) return [];

        var damageList = new List<DamageRecord>();

        var records = DamageRecords.Values.ToArray();
        Array.Sort(records, (a, b) => b.Time.CompareTo(a.Time));

        var count = Math.Min(deathConfiguration.MaxDeathRecords, records.Length);

        foreach (var record in records)
        {
            if (count >= deathConfiguration.MaxDeathRecords) break;
            if (record.NumberOfHits < deathConfiguration.DeathAssistCount) continue;
            if (record.Time >= DateTime.Now.Ticks - deathConfiguration.DeathListRequiredTime) continue;

            damageList.Add(record);
            count++;
        }

        return damageList;
    }
}