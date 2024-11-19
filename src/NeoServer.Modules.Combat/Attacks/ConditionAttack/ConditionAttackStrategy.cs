using NeoServer.Game.Combat;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Modules.Combat.Attacks.ConditionAttack;

public sealed class ConditionAttackStrategy
    : AttackStrategy
{
    public override string Name { get; } = nameof(ConditionAttackStrategy);

    /// <summary>
    /// Executes the melee attack, validating the attack and applying damage to the target.
    /// </summary>
    protected override Result Attack(in AttackInput attackInput)
    {
        var toCreature = attackInput.Target;
        if (toCreature is not ICombatActor victim) return Result.NotPossible;

        var damages = attackInput.Parameters.Damage;

        if (damages.Max == 0) return Result.Success;
        var conditionType = ConditionTypeParser.Parse(attackInput.Parameters.DamageType);

        var (damageCount, interval) = attackInput.Parameters.Condition;

        if (victim.HasCondition(conditionType, out var condition) && condition is DamageCondition damageCondition)
        {
            if (attackInput.Parameters.Condition.DamageCount == 0)
                damageCondition.Start(victim, (ushort)damages.Min, (ushort)damages.Max);
            else damageCondition.Restart(damageCount);
        }
        else
        {
            if (damageCount == 0)
                victim.AddCondition(new DamageCondition(conditionType, interval, (ushort)damages.Min,
                    (ushort)damages.Max));
            else
                victim.AddCondition(new DamageCondition(conditionType, interval, damageCount, (ushort)damages.Min));
        }

        return Result.Success;
    }
}