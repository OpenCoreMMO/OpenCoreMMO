using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Combat;

public readonly ref struct DamageResult(CombatDamageList damageList, bool wasDamaged)
{
    public CombatDamageList DamageList { get; } = damageList;
    public bool WasDamaged { get; } = wasDamaged;
}