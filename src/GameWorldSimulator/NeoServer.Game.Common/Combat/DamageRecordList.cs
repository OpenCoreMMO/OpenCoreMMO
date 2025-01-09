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
            record.IncreaseDamage(damage);
            record.UpdateTime();
            return;
        }

        DamageRecords.Add(key, new DamageRecord(thing, damage));
    }

    public Span<DamageRecord> GetLastDamageRecords(int count = int.MaxValue)
    {
        if (count <= 0) return Span<DamageRecord>.Empty;

        var records = DamageRecords.Values.ToArray();
        Array.Sort(records, (a, b) => b.Time.CompareTo(a.Time));

        count = Math.Min(count, records.Length);
        return new Span<DamageRecord>(records, 0, count);
    }
}