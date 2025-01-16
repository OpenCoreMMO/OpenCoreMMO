using System;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Combat;

public class DamageRecord(IThing aggressor, ushort damage)
{
    public ushort Damage { get; private set; } = damage;
    public ushort NumberOfHits { get; private set; } = 1;
    public long LastDamageTime { get; private set; } = DateTime.Now.Ticks;
    public long FirstDamageTime { get; private set; }
    public void AddDamage(ushort damage)
    {
        if (FirstDamageTime is 0) FirstDamageTime = DateTime.Now.Ticks;
        
        Damage += damage;
        
        LastDamageTime = DateTime.Now.Ticks;
        NumberOfHits++;
    }
    public IThing Aggressor { get; } = aggressor;
}