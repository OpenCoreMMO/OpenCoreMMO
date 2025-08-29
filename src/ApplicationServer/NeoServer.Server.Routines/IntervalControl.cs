using System;

namespace NeoServer.Server.Routines;

public class IntervalControl
{
    private readonly int interval;
    private DateTime lastRun;

    public IntervalControl(int interval)
    {
        this.interval = interval;
    }

    public void MarkAsExecuted()
    {
        lastRun = DateTime.UtcNow;
    }

    public bool CanExecuteNow()
    {
        return DateTime.UtcNow >= lastRun.AddMilliseconds(interval);
    }
}