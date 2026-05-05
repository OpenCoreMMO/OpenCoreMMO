using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Domain.Combat.Attacks;

public class ConditionAttackService(IMonsterTypeStore monsterTypeStore) : IAttackService
{
    private static readonly HashSet<ConditionType> HarmfulConditions =
    [
        ConditionType.Bleeding,
        ConditionType.Freezing,
        ConditionType.Burning,
        ConditionType.Poisoned,
        ConditionType.Electrified,
        ConditionType.Drowning,
        ConditionType.Cursed,
        ConditionType.Dazzled
    ];

    public CombatResult Execute(AttackInput attackInput)
    {
        var combatParameter = attackInput.Parameters;
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = (attackInput.Target ?? aggressor?.CurrentTarget) as ICombatActor;

        if (target is not null &&
            combatParameter.Condition is not null &&
            combatParameter.Condition.Type is not ConditionType.None)
        {
            var isDamageCondition = HarmfulConditions.Contains(combatParameter.Condition.Type);
            if (isDamageCondition) return PerformDamageCondition(combatParameter, target, aggressor);
            return PerformCondition(combatParameter, target);
        }

        if (combatParameter.Conditions.Count > 0)
        {
            foreach (var condition in combatParameter.Conditions)
            {
                target ??= aggressor;

                var isDamageCondition = HarmfulConditions.Contains(condition.Type);
                if (isDamageCondition) return PerformDamageCondition(combatParameter, target, aggressor, condition);

                PerformCondition(combatParameter, target, condition);
            }

            return new CombatResult(0, Result.Success);
        }

        return CombatResult.Fail(Result.NotApplicable);
    }

    private CombatResult PerformCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ICondition condition)
    {
        var conditionType = condition.Type;
        condition.Parameters.TryGetValue(ConditionParamType.Ticks, out var duration);

        if (!targetCreature.HasCondition(condition.Type, out var existentCondition))
        {
            if (conditionType is ConditionType.Light)
            {
                AddLightCondition(targetCreature, duration, condition);
                return new CombatResult(0, Result.Success);
            }

            if (conditionType is ConditionType.Haste)
            {
                AddSpeedCondition(targetCreature, duration, condition);
                return new CombatResult(0, Result.Success);
            }

            if (conditionType is ConditionType.Paralyze)
            {
                AddParalyzeCondition(combatParameter, targetCreature, duration);
                return new CombatResult(0, Result.Success);
            }

            if (conditionType is ConditionType.Outfit)
            {
                AddOutfitCondition(combatParameter, targetCreature, conditionType, duration);
                return new CombatResult(0, Result.Success);
            }

            targetCreature.AddCondition(new Condition(conditionType, duration));

            return new CombatResult(0, Result.Success);
        }

        //re-add condition
        targetCreature.RestartCondition(existentCondition);


        return new CombatResult(0, Result.Success);
    }

    private CombatResult PerformCondition(CombatParameter combatParameter, ICombatActor targetCreature)
    {
        var conditionType = combatParameter.Condition.Type;
        var duration = combatParameter.Condition.Duration;

        if (!targetCreature.HasCondition(combatParameter.Condition.Type, out var condition))
        {
            if (conditionType is ConditionType.Paralyze)
            {
                AddParalyzeCondition(combatParameter, targetCreature, duration);
                return new CombatResult(0, Result.Success);
            }

            if (conditionType is ConditionType.Outfit)
            {
                AddOutfitCondition(combatParameter, targetCreature, conditionType, duration);
                return new CombatResult(0, Result.Success);
            }

            targetCreature.AddCondition(new Condition(conditionType, duration));

            return new CombatResult(0, Result.Success);
        }

        //re-add condition
        targetCreature.RestartCondition(condition);

        return new CombatResult(0, Result.Success);
    }

    private static CombatResult PerformDamageCondition(
        CombatParameter combatParameter,
        ICombatActor targetCreature,
        ICombatActor aggressor,
        ICondition condition)
    {
        var conditionType = condition.Type;
        condition.Parameters.TryGetValue(ConditionParamType.Ticks, out var duration);

        if (combatParameter.MinDamage is 0 || combatParameter.MaxDamage is 0)
            return CombatResult.Fail(Result.NotPossible);

        targetCreature.RemoveCondition(conditionType);
        targetCreature.AddCondition(new ConditionDamage(aggressor, conditionType, duration,
            combatParameter.MinDamage,
            combatParameter.MaxDamage));

        return new CombatResult(0, Result.Success);
    }

    private static CombatResult PerformDamageCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ICombatActor aggressor)
    {
        var conditionType = combatParameter.Condition.Type;
        var interval = combatParameter.Condition.Duration;

        if (combatParameter.MinDamage is 0 || combatParameter.MaxDamage is 0)
            return CombatResult.Fail(Result.NotPossible);

        targetCreature.RemoveCondition(conditionType);
        targetCreature.AddCondition(new ConditionDamage(aggressor, conditionType, interval,
            combatParameter.MinDamage,
            combatParameter.MaxDamage));

        return new CombatResult(0, Result.Success);
    }

    private static void AddLightCondition(ICombatActor targetCreature, uint duration, ICondition condition)
    {
        condition.Parameters.TryGetValue(ConditionParamType.LightLevel, out var lightLevel);
        condition.Parameters.TryGetValue(ConditionParamType.LightColor, out var lightColor);

        targetCreature.AddCondition(new ConditionLight(duration, lightLevel, lightColor)
        {
            EndAction = targetCreature.RemoveLight
        });
    }

    private static void AddSpeedCondition(ICombatActor targetCreature, uint duration, ICondition condition)
    {
        targetCreature.AddCondition(new HasteCondition(duration, condition.FormulaValues));
    }

    private static void AddParalyzeCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        uint duration)
    {
        var speedReduction = (ushort)Math.Abs((int)combatParameter.Condition.Value);
        targetCreature.AddCondition(new ParalyzeCondition(duration, speedReduction));
    }


    private void AddOutfitCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ConditionType conditionType, uint duration)
    {
        monsterTypeStore.TryGetValue((string)combatParameter.Condition.Value, out var monster);

        monster.Look.TryGetValue(LookType.Type, out var lookType);
        monster.Look.TryGetValue(LookType.Addon, out var addon);
        monster.Look.TryGetValue(LookType.Head, out var head);
        monster.Look.TryGetValue(LookType.Body, out var body);
        monster.Look.TryGetValue(LookType.Legs, out var legs);
        monster.Look.TryGetValue(LookType.Feet, out var feet);

        targetCreature.AddCondition(new OutfitCondition(duration, lookType, (byte)head, (byte)body, (byte)legs,
            (byte)feet, (byte)addon));
    }
}