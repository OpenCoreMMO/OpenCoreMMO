using System;

namespace NeoServer.Game.Common.Creatures.Structs;

public struct CooldownTime
{
    public CooldownTime(DateTime start, uint duration)
    {
        Start = start.Ticks;
        Duration = TimeSpan.TicksPerMillisecond * duration;
    }

    public long Start { get; set; }
    public long Duration { get; set; }
    public bool Expired => Start + Duration <= DateTime.Now.Ticks;

    public TimeSpan Remaining
    {
        get
        {
            var remainingTicks = (Start + Duration) - DateTime.Now.Ticks;
            return remainingTicks > 0
                ? TimeSpan.FromTicks(remainingTicks)
                : TimeSpan.Zero;
        }
    }
    public void Reset()
    {
        Start = DateTime.Now.Ticks;
    }
}