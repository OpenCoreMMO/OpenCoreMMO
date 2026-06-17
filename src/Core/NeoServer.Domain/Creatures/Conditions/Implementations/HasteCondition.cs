using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class HasteCondition : BaseCondition
{
    public HasteCondition(
        uint interval,
        FormulaValues formulaValues,
        EffectT effect = EffectT.None) : base(interval)
    {
        Interval = interval;
        FormulaValues = formulaValues;
        Effect = effect;
    }

    public override ConditionType Type => ConditionType.Haste;
    public EffectT Effect { get; }
    public uint Interval { get; }

    internal ushort SpeedBoost { get; private set; }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        if (creature is not IWalkableCreature walkableCreature)
            return false;
        
        //End any existing paralyze conditions
        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Paralyze);
        }

        if (SpeedBoost == 0) // not yet set; generate
        {
            var baseSpeed = walkableCreature.RawSpeed;

            var min = walkableCreature.RawSpeed * FormulaValues.MinA + FormulaValues.MinB;
            var max = walkableCreature.RawSpeed * FormulaValues.MaxA + FormulaValues.MaxB;

            var random = Random.Shared;
            var randomSpeed = random.Next((int)min, (int)max);
            SpeedBoost = (ushort)(randomSpeed - baseSpeed);
        }

        walkableCreature.IncreaseSpeed(SpeedBoost);

        EndAction = () => walkableCreature.DecreaseSpeed(SpeedBoost);

        return true;
    }

    public HasteConditionState CaptureState()
    {
        return new HasteConditionState(
            Type,
            Effect,
            SpeedBoost,
            Math.Max(0, RemainingTime),
            Duration,
            FormulaValues);
    }

    public static HasteCondition Restore(HasteConditionState state)
    {
        // Use remaining time so the condition expires at the correct moment.
        // If remaining is 0 (expired), fall back to 1ms
        // as a reasonable default.
        var durationMs = state.RemainingTimeMilliseconds > 0
            ? state.RemainingTimeMilliseconds
            : 1;

        var condition = new HasteCondition(
            (uint)durationMs,
            state.FormulaValues,
            state.Effect)
        {
            SpeedBoost = state.SpeedBoost
        };

        return condition;
    }
}

public sealed record HasteConditionState(
    ConditionType Type,
    EffectT Effect,
    ushort SpeedBoost,
    long RemainingTimeMilliseconds,
    long Duration,
    FormulaValues FormulaValues);