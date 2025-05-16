using System;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Combat.Attacks;

public struct MonsterCombatAttack : IMonsterCombatAttack
{
    public int Interval
    {
        set => Cooldown = new CooldownTime(DateTime.Now, (uint)value);
    }

    public byte Chance { get; set; }
    public byte Target { get; set; }
    public DamageType DamageType { get; set; }
    public ushort MinDamage { get; set; }
    public ushort MaxDamage { get; set; }
    public bool IsMelee => DamageType == DamageType.Melee;
    public ICombatAttack CombatAttack { get; set; }

    public CooldownTime Cooldown { get; private set; }

    public CombatAttackValue Translate()
    {
        if (CombatAttack is DistanceCombatAttack distance)
            return new CombatAttackValue(MinDamage, MaxDamage, distance.Range, DamageType);

        return new CombatAttackValue(MinDamage, MaxDamage, DamageType);
    }
}