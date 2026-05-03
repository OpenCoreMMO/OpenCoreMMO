using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionRegeneration : BaseCondition
{
    private const uint MaxDurationMs = 1200 * 1000; // 20 minutes
    private bool _ended;

    public ConditionRegeneration(uint duration, Action onExpired) : base(duration)
    {
        Type = ConditionType.Regeneration;
        EndAction = onExpired;
    }

    public override ConditionType Type { get; }

    public override void End()
    {
        if (_ended) return;
        _ended = true;
        base.End();
    }

    public bool TryExtend(uint additionalMs)
    {
        if (RemainingTime + additionalMs >= MaxDurationMs) return false;
        Extend(additionalMs, MaxDurationMs);
        return true;
    }
}
