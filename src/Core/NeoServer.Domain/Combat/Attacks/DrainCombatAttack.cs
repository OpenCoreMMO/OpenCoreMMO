using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Combat.Attacks;

public class DrainCombatAttack : DistanceAreaCombatAttack
{
    public DrainCombatAttack(byte range, byte radius, ShootType shootType) : base(range, radius, shootType)
    {
    }

    public override bool TryAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option,
        out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(ShootType)
        {
            EffectT = option.DamageEffect
        };

        if (CalculateAttack(actor, enemy, option, out var damage))
        {
            combatResult.DamageType = option.DamageType;

            enemy.TakeDamage(actor, damage);
            return true;
        }

        return false;
    }
}