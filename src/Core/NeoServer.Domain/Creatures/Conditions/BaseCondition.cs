using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions;

public abstract class BaseCondition : ICondition
{
    protected BaseCondition(uint duration)
    {
        Duration = duration * TimeSpan.TicksPerMillisecond;
    }

    protected BaseCondition(uint duration, Action onEndAction)
    {
        Duration = duration * TimeSpan.TicksPerMillisecond;
        EndAction = onEndAction;
    }

    public Action EndAction { private get; set; }
    public long Duration { get; private set; }
    public long EndTime { get; private set; }

    public bool IsPersistent => Duration == 0;
    public long StartedAt { get; private set; }
    public bool IsDisabled { get; private set; }
    
    public ConditionIconType Icons => 0;

    public abstract ConditionType Type { get; }
    public long RemainingTime => (EndTime - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerMillisecond;

    public FormulaValues FormulaValues { get; set; }
    public Dictionary<ConditionParamType, uint> Parameters { get; set; } = new();

    private bool _hasEnded;

    internal virtual void End()
    {
        if (_hasEnded || IsPersistent) return;
        _hasEnded = true;

        EndAction?.Invoke(); //can cause side effect
    }

    public virtual void Extend(uint duration, uint maxDuration = uint.MaxValue)
    {
        var maxDurationTicks = maxDuration * TimeSpan.TicksPerMillisecond;
        var durationTicks = duration * TimeSpan.TicksPerMillisecond;

        if (Duration + durationTicks > maxDurationTicks) return;

        Duration += durationTicks;
        EndTime += durationTicks;
    }

    public void Disable()
    {
        IsDisabled = true;
    }

    public void Enable()
    {
        IsDisabled = false;
    }

    internal virtual bool Start(ICreature creature)
    {
        if (Duration == 0) return true;
        
        StartedAt = DateTime.UtcNow.Ticks;
        EndTime = StartedAt + Duration;
        return true;
    }

    internal virtual bool Restart(ICreature creature)
    {
        _hasEnded = false;
        Start(creature);
        return true;
    }

    public virtual bool HasExpired => !IsPersistent && EndTime < DateTime.UtcNow.Ticks;
    
    /// <summary>
    ///     Updates the duration of the condition. If the condition has already started,
    ///     the end time is recalculated from the new duration.
    /// </summary>
    /// <param name="duration">The new duration in milliseconds.</param>
    public void SetNewDuration(uint duration)
    {
        Duration = duration * TimeSpan.TicksPerMillisecond;
        if (StartedAt > 0) EndTime = StartedAt + Duration;
    }

    /// <summary>
    ///     Updates the duration of the condition. If the condition has already started,
    ///     the end time is recalculated from the new duration.
    /// </summary>
    /// <param name="duration">The new duration in ticks.</param>
    public void SetNewDuration(long duration)
    {
        Duration = duration;
        if (StartedAt > 0) EndTime = StartedAt + Duration;
    }
}