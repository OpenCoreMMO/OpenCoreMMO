#nullable enable
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ParalyzeCondition : BaseCondition
{
    internal ushort SpeedReduction { get; private set; }

    public override ConditionType Type => ConditionType.Paralyze;

    public ParalyzeCondition(uint duration, ushort speedReduction) : base(duration)
    {
        SpeedReduction = speedReduction;
    }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is not IWalkableCreature walkable) return true;
        
        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Haste);
        }

        walkable.DecreaseSpeed(SpeedReduction);

        EndAction = () => walkable.IncreaseSpeed(SpeedReduction);
        
        return true;
    }

    public ParalyzeConditionState CaptureState()
    {
        return new ParalyzeConditionState(
            Type,
            SpeedReduction,
            Math.Max(0, RemainingTime));
    }

    public static ParalyzeCondition? Restore(ParalyzeConditionState state)
    {
        var durationMs = Math.Max(0, state.RemainingTimeMilliseconds);

        if (durationMs <= 0)
            return null;

        return new ParalyzeCondition(
            (uint)durationMs,
            state.SpeedReduction);
    }
}

public sealed record ParalyzeConditionState(
    ConditionType Type,
    ushort SpeedReduction,
    long RemainingTimeMilliseconds);
