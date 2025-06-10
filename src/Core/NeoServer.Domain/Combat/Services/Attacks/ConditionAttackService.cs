using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class ConditionAttackService(IMonsterDataManager monsterDataManager) : IAttackService
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

    public Result Execute(AttackInput attackInput)
    {
        var combatParameter = attackInput.Parameters;
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target ?? aggressor?.CurrentTarget;

        if (target is not ICombatActor targetCreature || combatParameter.Condition is null ||
            combatParameter.Condition.Type is ConditionType.None) return Result.NotApplicable;

        var isDamageCondition = HarmfulConditions.Contains(combatParameter.Condition.Type);
        if (isDamageCondition) return PerformDamageCondition(combatParameter, targetCreature, aggressor);

        return PerformCondition(combatParameter, targetCreature);
    }

    private static Result PerformDamageCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ICombatActor aggressor)
    {
        var conditionType = combatParameter.Condition.Type;
        var interval = combatParameter.Condition.Duration;

        if (combatParameter.MinDamage is 0 || combatParameter.MaxDamage is 0) return Result.NotPossible;

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

    private Result PerformCondition(CombatParameter combatParameter, ICombatActor targetCreature)
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

            if (conditionType is ConditionType.Outfit)
            {
                AddOutfitCondition(combatParameter, targetCreature, conditionType, duration);
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
        targetCreature.DecreaseSpeed((ushort)Math.Abs((int)combatParameter.Condition.Value));

        targetCreature.AddCondition(new Condition(conditionType, duration)
        {
            EndAction = () => targetCreature.IncreaseSpeed((ushort)Math.Abs((int)combatParameter.Condition.Value))
        });
    }

    private void AddOutfitCondition(CombatParameter combatParameter, ICombatActor targetCreature,
        ConditionType conditionType, uint duration)
    {
        monsterDataManager.TryGetMonster((string)combatParameter.Condition.Value, out var monster);

        monster.Look.TryGetValue(LookType.Type, out var lookType);
        monster.Look.TryGetValue(LookType.Addon, out var addon);
        monster.Look.TryGetValue(LookType.Head, out var head);
        monster.Look.TryGetValue(LookType.Body, out var body);
        monster.Look.TryGetValue(LookType.Legs, out var legs);
        monster.Look.TryGetValue(LookType.Feet, out var feet);

        targetCreature.SetTemporaryOutfit(lookType, (byte)head, (byte)body, (byte)legs, (byte)feet, (byte)addon);

        targetCreature.AddCondition(new Condition(conditionType, duration)
        {
            EndAction = targetCreature.BackToOldOutfit
        });
    }
}