using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Effects.Magical;

namespace NeoServer.Domain.Combat.Attacks;

public class AreaCombatAttack : CombatAttack
{
    private readonly byte[,] _area;

    public AreaCombatAttack(byte[,] area)
    {
        _area = area;
    }

    public override bool TryAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option,
        out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult();

        if (CalculateAttack(actor, enemy, option, out var damage))
        {
            combatResult.DamageType = option.DamageType;
            combatResult.EffectT = option.DamageEffect;

            var area = AreaEffect.Create(actor.Location, _area);
            combatResult.SetArea(area);
            actor.PropagateAttack(combatResult.Area, damage);
            return true;
        }

        return false;
    }
}