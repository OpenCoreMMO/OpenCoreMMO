﻿using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class GlobalEvents : IGlobalEvents
{
    private int SchedulerMinTicks = 50;

    private readonly Dictionary<string, GlobalEvent> _thinkMap = new Dictionary<string, GlobalEvent>();
    private readonly Dictionary<string, GlobalEvent> _serverMap = new Dictionary<string, GlobalEvent>();
    private readonly Dictionary<string, GlobalEvent> _timerMap = new Dictionary<string, GlobalEvent>();

    private uint thinkEventId = 0;
    private uint timerEventId = 0;

    private ILogger _logger;
    private IScheduler _scheduler;

    public GlobalEvents(
        ILogger logger,
        IScheduler dispatcher)
    {
        _logger = logger;
        _scheduler = dispatcher;
    }

    public void Startup()
    {
        Execute(GlobalEventType.GLOBALEVENT_STARTUP);
    }

    public void Shutdown()
    {
        Execute(GlobalEventType.GLOBALEVENT_SHUTDOWN);
    }

    public void Save()
    {
        Execute(GlobalEventType.GLOBALEVENT_SAVE);
    }

    public void Timer()
    {
        var now = DateTime.Now;
        var nextScheduledTime = long.MaxValue;

        foreach (var globalEvent in _timerMap.Values.ToList())
        {
            var nextExecutionTime = globalEvent.NextExecution - now.Ticks;
            if (nextExecutionTime > 0)
            {
                if (nextExecutionTime < nextScheduledTime)
                {
                    nextScheduledTime = nextExecutionTime;
                }

                continue;
            }

            if (!globalEvent.ExecuteEvent())
            {
                _timerMap.Remove(globalEvent.Name);
                continue;
            }

            nextExecutionTime = 86400;
            if (nextExecutionTime < nextScheduledTime)
            {
                nextScheduledTime = nextExecutionTime;
            }

            globalEvent.NextExecution += nextExecutionTime;
        }

        if (nextScheduledTime != long.MaxValue)
        {
            var delay = int.Max(1000, (int)nextScheduledTime * 1000);
            thinkEventId = _scheduler.AddEvent(new SchedulerEvent(delay, Think));
        }
    }

    public void Think()
    {
        //TotalMilliseconds
        var now = _scheduler.GlobalTime;
        var nextScheduledTime = long.MaxValue;

        foreach (var globalEvent in _thinkMap.Values.ToList())
        {
            var nextExecutionTime = globalEvent.NextExecution - now;
            if (nextExecutionTime > 0)
            {
                if (nextExecutionTime < nextScheduledTime)
                {
                    nextScheduledTime = nextExecutionTime;
                }

                continue;
            }

            _logger.Debug($"[GlobalEvents::think] - Executing event: {globalEvent.Name}");

            if (!globalEvent.ExecuteEvent())
            {
                _logger.Information($"[GlobalEvents::think] - Failed to execute event: {globalEvent.Name}");
            }

            nextExecutionTime = globalEvent.Interval;
            if (nextExecutionTime < nextScheduledTime)
            {
                nextScheduledTime = nextExecutionTime;
            }

            globalEvent.NextExecution += nextExecutionTime;
        }

        if (nextScheduledTime != long.MaxValue)
        {
            var delay = (int)nextScheduledTime;
            thinkEventId = _scheduler.AddEvent(new SchedulerEvent(delay, Think));
        }
    }

    public void Execute(GlobalEventType type)
    {
        foreach (var globalEvent in _serverMap.Values)
        {
            if (globalEvent.EventType == type)
            {
                globalEvent.ExecuteEvent();
            }
        }
    }

    public Dictionary<string, GlobalEvent> GetEventMap(GlobalEventType type)
    {
        switch (type)
        {
            case GlobalEventType.GLOBALEVENT_NONE:
                return _thinkMap;
            case GlobalEventType.GLOBALEVENT_TIMER:
                return _timerMap;
            case GlobalEventType.GLOBALEVENT_PERIODCHANGE:
            case GlobalEventType.GLOBALEVENT_STARTUP:
            case GlobalEventType.GLOBALEVENT_SHUTDOWN:
            case GlobalEventType.GLOBALEVENT_RECORD:
            case GlobalEventType.GLOBALEVENT_SAVE:
                var retMap = new Dictionary<string, GlobalEvent>();
                foreach (var kvp in _serverMap)
                {
                    if (kvp.Value.EventType == type)
                    {
                        retMap.Add(kvp.Key, kvp.Value);
                    }
                }
                return retMap;
            default:
                return new Dictionary<string, GlobalEvent>();
        }
    }

    public bool RegisterLuaEvent(GlobalEvent globalEvent)
    {
        if (globalEvent.EventType == GlobalEventType.GLOBALEVENT_TIMER)
        {
            if (_timerMap.TryAdd(globalEvent.Name, globalEvent))
            {
                if (timerEventId == 0)
                {
                    timerEventId = _scheduler.AddEvent(new SchedulerEvent(SchedulerMinTicks, Timer));
                }
                return true;
            }
        }
        else if (globalEvent.EventType != GlobalEventType.GLOBALEVENT_NONE)
        {
            if (_serverMap.TryAdd(globalEvent.Name, globalEvent))
            {
                return true;
            }
        }
        else // think event
        {
            if (_thinkMap.TryAdd(globalEvent.Name, globalEvent))
            {
                if (thinkEventId == 0)
                {
                    thinkEventId = _scheduler.AddEvent(new SchedulerEvent(SchedulerMinTicks, Think));
                }
                return true;
            }
        }

        _logger.Information($"Duplicate registered globalevent with name: {globalEvent.Name}");
        return false;
    }

    public void Clear()
    {
        // Stop events
        _scheduler.CancelEvent(thinkEventId);
        thinkEventId = 0;
        _scheduler.CancelEvent(timerEventId);
        timerEventId = 0;

        // Clear maps
        _thinkMap.Clear();
        _serverMap.Clear();
        _timerMap.Clear();
    }
}
