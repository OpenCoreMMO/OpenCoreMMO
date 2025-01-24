using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Combat;

public class DamageRecord(IThing aggressor, ushort damage, bool unjustified)
{
    public ushort Damage { get; private set; } = damage;
    public ushort NumberOfHits { get; private set; } = 1;
    public long LastDamageTime { get; private set; } = DateTime.Now.Ticks;
    public long FirstDamageTime { get; private set; }
    public bool Unjustified { get; private set; } = unjustified;

    public void AddDamage(ushort damage, bool unjustified)
    {
        if (FirstDamageTime is 0) FirstDamageTime = DateTime.Now.Ticks;

        Damage += damage;

        LastDamageTime = DateTime.Now.Ticks;
        Unjustified = unjustified;
        NumberOfHits++;
    }

    public IThing Aggressor { get; } = aggressor;
}

public record DamageRecordResult(List<DamageRecord> DamageRecords, bool HasUnjustifiedDamage);