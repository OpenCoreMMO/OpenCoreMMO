using System;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Combat;

public class DamageRecord(IThing aggressor, ushort damage)
{
    public ushort Damage { get; private set; } = damage;
    public ushort NumberOfHits { get; private set; } = 1;
    public long Time { get; private set; } = DateTime.Now.Ticks;
    public void AddDamage(ushort damage)
    {
        Damage += damage;
        
        Time = DateTime.Now.Ticks;
        NumberOfHits++;
    }
    public IThing Aggressor { get; } = aggressor;
}