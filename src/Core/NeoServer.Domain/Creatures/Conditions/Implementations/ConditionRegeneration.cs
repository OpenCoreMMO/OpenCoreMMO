#nullable enable
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionRegeneration : BaseCondition
{
    private const uint MAX_DURATION_MS = 1200 * 1000; // 20 minutes

    public ConditionRegeneration(uint duration) : base(duration)
    {
        Type = ConditionType.Regeneration;
    }

    public override ConditionType Type { get; }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;
        if (creature is IPlayer player)
            EndAction = player.SetAsHungry;
        return true;
    }

    public ConditionRegenerationState CaptureState()
    {
        return new ConditionRegenerationState(
            Type,
            Math.Max(0, RemainingTime));
    }

    public static ConditionRegeneration? Restore(ConditionRegenerationState state)
    {
        var durationMs = Math.Max(0, state.RemainingTimeMilliseconds);

        if (durationMs <= 0)
            return null;

        return new ConditionRegeneration((uint)durationMs);
    }

    public bool TryExtend(uint additionalMs)
    {
        var effectiveRemainingMs = Math.Max(0, RemainingTime);
        if (effectiveRemainingMs + additionalMs >= MAX_DURATION_MS) return false;

        if (EndTime == 0 && Duration > 0) Start(default!);

        Extend(additionalMs, MAX_DURATION_MS);
        return true;
    }
}

public sealed record ConditionRegenerationState(
    ConditionType Type,
    long RemainingTimeMilliseconds);
