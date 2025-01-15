using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Conditions;

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

    public ConditionIcon Icons => 0;

    public abstract ConditionType Type { get; }
    public long RemainingTime => (EndTime - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;

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

    public virtual bool Start(ICreature creature)
    {
        StartedAt = DateTime.Now.Ticks;
        EndTime = DateTime.Now.Ticks + Duration;

        return true;
    }

    public virtual bool HasExpired => IsPersistent is false && EndTime < DateTime.Now.Ticks;
}