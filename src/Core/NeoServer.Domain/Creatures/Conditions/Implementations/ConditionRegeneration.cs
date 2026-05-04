using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionRegeneration : BaseCondition
{
    private const uint MaxDurationMs = 1200 * 1000; // 20 minutes

    public ConditionRegeneration(uint duration, Action onExpired) : base(duration)
    {
        Type = ConditionType.Regeneration;
        EndAction = onExpired;
    }

    public override ConditionType Type { get; }

    public bool TryExtend(uint additionalMs)
    {
        if (RemainingTime + additionalMs >= MaxDurationMs) return false;
        Extend(additionalMs, MaxDurationMs);
        return true;
    }
}
