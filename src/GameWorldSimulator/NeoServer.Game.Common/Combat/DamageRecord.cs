using System;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Combat;

public class DamageRecord(IThing aggressor, ushort damage)
{
    public ushort Damage { get; private set; } = damage;
    public long Time { get; private set; } = DateTime.Now.Ticks;
    public void UpdateTime() => Time = DateTime.Now.Ticks;
    public void IncreaseDamage(ushort damage) => Damage += damage;
    public IThing Aggressor { get; } = aggressor;
}