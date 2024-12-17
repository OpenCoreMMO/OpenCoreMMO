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
        lastRun = DateTime.Now;
    }

    public bool CanExecuteNow()
    {
        return DateTime.Now >= lastRun.AddMilliseconds(interval);
    }
}