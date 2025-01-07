using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class GlobalEvents(
    ILogger logger,
    IScheduler dispatcher) : IGlobalEvents
{
    private const int SCHEDULER_MIN_TICKS = 50;

    private readonly Dictionary<string, GlobalEvent> _thinkMap = new();
    private readonly Dictionary<string, GlobalEvent> _serverMap = new();
    private readonly Dictionary<string, GlobalEvent> _timerMap = new();

    private uint _thinkEventId;
    private uint _timerEventId;

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
            _thinkEventId = dispatcher.AddEvent(new SchedulerEvent(delay, Think));
        }
    }

    public void Think()
    {
        //TotalMilliseconds
        var now = dispatcher.GlobalTime;
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

            logger.Debug("[GlobalEvents::think] - Executing event: {GlobalEventName}", globalEvent.Name);

            if (!globalEvent.ExecuteEvent())
            {
                logger.Information("[GlobalEvents::think] - Failed to execute event: {GlobalEventName}", globalEvent.Name);
            }

            nextExecutionTime = globalEvent.Interval;
            if (nextExecutionTime < nextScheduledTime)
            {
                nextScheduledTime = nextExecutionTime;
            }

            globalEvent.NextExecution += nextExecutionTime;
        }

        if (nextScheduledTime == long.MaxValue) return;
        
        var delay = (int)nextScheduledTime;
        _thinkEventId = dispatcher.AddEvent(new SchedulerEvent(delay, Think));
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
                if (_timerEventId == 0)
                {
                    _timerEventId = dispatcher.AddEvent(new SchedulerEvent(SCHEDULER_MIN_TICKS, Timer));
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
                if (_thinkEventId == 0)
                {
                    _thinkEventId = dispatcher.AddEvent(new SchedulerEvent(SCHEDULER_MIN_TICKS, Think));
                }
                return true;
            }
        }

        logger.Information("Duplicate registered GlobalEvent with name: {GlobalEventName}", globalEvent.Name);
        return false;
    }

    public void Clear()
    {
        // Stop events
        dispatcher.CancelEvent(_thinkEventId);
        _thinkEventId = 0;
        dispatcher.CancelEvent(_timerEventId);
        _timerEventId = 0;

        // Clear maps
        _thinkMap.Clear();
        _serverMap.Clear();
        _timerMap.Clear();
    }
}
