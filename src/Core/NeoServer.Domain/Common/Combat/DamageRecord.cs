using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Combat;

public class DamageRecord(IThing aggressor, ushort damage, bool unjustified)
{
    public ushort Damage { get; private set; } = damage;
    public ushort NumberOfHits { get; private set; } = 1;
    public long LastDamageTime { get; private set; } = DateTime.UtcNow.Ticks;
    public long FirstDamageTime { get; private set; }
    public bool Unjustified { get; private set; } = unjustified;

    public IThing Aggressor { get; } = aggressor;

    public void AddDamage(ushort damage, bool unjustified)
    {
        if (FirstDamageTime is 0) FirstDamageTime = DateTime.UtcNow.Ticks;

        Damage += damage;

        LastDamageTime = DateTime.UtcNow.Ticks;
        Unjustified = unjustified;
        NumberOfHits++;
    }
}

public record DamageRecordResult(List<DamageRecord> DamageRecords, bool HasUnjustifiedDamage);