using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class ConditionAttackService : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        var combatParameter = attackInput.Parameters;
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target ?? aggressor?.CurrentTarget;

        if (target is not ICombatActor targetCreature || combatParameter.Condition is null ||
            combatParameter.Condition.Type is ConditionType.None) return Result.NotApplicable;

        var isDamageCondition = combatParameter.MinDamage > 0 || combatParameter.MaxDamage > 0;

        if (isDamageCondition)
        {
            return PerformDamageCondition(combatParameter, targetCreature, aggressor);
        }

        return PerformCondition(combatParameter, targetCreature);
    }

    private static Result PerformDamageCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ICombatActor aggressor)
    {
        var conditionType = combatParameter.Condition.Type;
        var interval = combatParameter.Condition.Duration;

        if (!targetCreature.HasCondition(combatParameter.Condition.Type, out var condition))
        {
            targetCreature.AddCondition(new DamageCondition(aggressor, conditionType, interval,
                combatParameter.MinDamage,
                combatParameter.MaxDamage));

            return Result.Success;
        }

        (condition as DamageCondition)?.Start(targetCreature, combatParameter.MinDamage, combatParameter.MaxDamage);
        return Result.Success;
    }

    private static Result PerformCondition(CombatParameter combatParameter, ICombatActor targetCreature)
    {
        var conditionType = combatParameter.Condition.Type;
        var duration = combatParameter.Condition.Duration;

        if (!targetCreature.HasCondition(combatParameter.Condition.Type, out var condition))
        {
            if (conditionType is ConditionType.Paralyze)
            {
                AddParalyzeCondition(combatParameter, targetCreature, conditionType, duration);
                return Result.Success;
            }

            targetCreature.AddCondition(new Condition(conditionType, duration));

            return Result.Success;
        }

        condition.Start(targetCreature);
        return Result.Success;
    }

    private static void AddParalyzeCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ConditionType conditionType, uint duration)
    {
        targetCreature.DecreaseSpeed((ushort)Math.Abs(combatParameter.Condition.Value));

        targetCreature.AddCondition(new Condition(conditionType, duration)
        {
            EndAction = () => targetCreature.IncreaseSpeed((ushort)Math.Abs(combatParameter.Condition.Value))
        });
    }
}