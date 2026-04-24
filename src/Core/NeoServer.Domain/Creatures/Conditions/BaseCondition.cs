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

    public void End()
    {
        if (IsPersistent) return;

        EndAction?.Invoke();
    }

    public virtual void Extend(uint duration, uint maxDuration = uint.MaxValue)
    {
        var maxDurationTicks = maxDuration * TimeSpan.TicksPerMillisecond;
        var durationTicks = duration * TimeSpan.TicksPerMillisecond;

        Duration += durationTicks;

        if (Duration > maxDurationTicks) return;

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

    public virtual bool Start(ICreature creature)
    {
        if (Duration == 0) return true;
        
        StartedAt = DateTime.UtcNow.Ticks;
        EndTime = StartedAt + Duration;
        return true;
    }

    public virtual bool HasExpired => !IsPersistent && EndTime < DateTime.UtcNow.Ticks;
    
    /// <summary>
    /// Updates the duration of the condition by setting a new value.
    /// </summary>
    /// <param name="duration">The new duration in milliseconds. This value is converted to ticks internally.</param>
    public void SetNewDuration(uint duration) => Duration = duration * TimeSpan.TicksPerMillisecond;

    /// <summary>
    /// Updates the duration of the condition by setting a new value.
    /// </summary>
    /// <param name="duration">The new duration in ticks.</param>
    public void SetNewDuration(long duration) => Duration = duration;
}