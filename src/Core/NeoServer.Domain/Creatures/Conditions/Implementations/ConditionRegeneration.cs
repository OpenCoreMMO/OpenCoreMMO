using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionRegeneration : BaseCondition
{
    private const uint MAX_DURATION_MS = 1200 * 1000; // 20 minutes

    public ConditionRegeneration(uint duration, Action onExpired) : base(duration)
    {
        Type = ConditionType.Regeneration;
        EndAction = onExpired;
    }

    public override ConditionType Type { get; }

    public bool TryExtend(uint additionalMs)
    {
        var effectiveRemainingMs = Math.Max(0, RemainingTime);
        if (effectiveRemainingMs + additionalMs >= MAX_DURATION_MS) return false;

        if (EndTime == 0 && Duration > 0) Start(null);

        Extend(additionalMs, MAX_DURATION_MS);
        return true;
    }
}
