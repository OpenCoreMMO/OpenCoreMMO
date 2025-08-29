namespace NeoServer.Domain.Common.Creatures.Structs;

public struct CooldownTime
{
    public CooldownTime(DateTime start, uint duration)
    {
        Start = start.Ticks;
        Duration = TimeSpan.TicksPerMillisecond * duration;
    }

    public long Start { get; set; }
    public long Duration { get; set; }
    public bool Expired => Start + Duration <= DateTime.UtcNow.Ticks;

    public TimeSpan Remaining
    {
        get
        {
            var remainingTicks = Start + Duration - DateTime.UtcNow.Ticks;
            return remainingTicks > 0
                ? TimeSpan.FromTicks(remainingTicks)
                : TimeSpan.Zero;
        }
    }

    public void Reset()
    {
        Start = DateTime.UtcNow.Ticks;
    }
}