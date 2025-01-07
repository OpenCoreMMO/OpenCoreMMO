using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GlobalEventFunctions : LuaScriptInterface, IGlobalEventFunctions
{
    private static ILogger _logger;
    private static IGlobalEvents _globalEvents;
    private static IDispatcher _dispatcher;

    public GlobalEventFunctions(
        ILogger logger, 
        IGlobalEvents globalEvents,
        IDispatcher dispatcher) : base(nameof(GlobalEventFunctions))

    {
        _logger = logger;
        _globalEvents = globalEvents;
        _dispatcher = dispatcher;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "GlobalEvent", "", LuaCreateGlobalEvent);
        RegisterMethod(luaState, "GlobalEvent", "type", LuaGlobalEventType);
        RegisterMethod(luaState, "GlobalEvent", "register", LuaGlobalEventRegister);
        RegisterMethod(luaState, "GlobalEvent", "time", LuaGlobalEventTime);
        RegisterMethod(luaState, "GlobalEvent", "interval", LuaGlobalEventInterval);
        RegisterMethod(luaState, "GlobalEvent", "onThink", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onTime", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onStartup", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onShutdown", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onRecord", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onPeriodChange", LuaGlobalEventOnCallback);
        RegisterMethod(luaState, "GlobalEvent", "onSave", LuaGlobalEventOnCallback);
    }

    private static int LuaCreateGlobalEvent(LuaState luaState)
    {
        var global = new GlobalEvent(GetScriptEnv().GetScriptInterface(), _logger);
        global.Name = GetString(luaState, 2);
        global.EventType = GlobalEventType.GLOBALEVENT_NONE;
        PushUserdata(luaState, global);
        SetMetatable(luaState, -1, "GlobalEvent");
        return 1;
    }

    private static int LuaGlobalEventType(LuaState luaState)
    {
        // globalevent:type(callback)
        var global = GetUserdata<GlobalEvent>(luaState, 1);
        if (global != null)
        {
            string typeName = GetString(luaState, 2);
            string tmpStr = typeName.ToLower();
            if (tmpStr == "startup")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_STARTUP;
            }
            else if (tmpStr == "shutdown")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_SHUTDOWN;
            }
            else if (tmpStr == "record")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_RECORD;
            }
            else if (tmpStr == "periodchange")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_PERIODCHANGE;
            }
            else if (tmpStr == "onthink")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_ON_THINK;
            }
            else if (tmpStr == "save")
            {
                global.EventType = GlobalEventType.GLOBALEVENT_SAVE;
            }
            else
            {
                _logger.Error("[GlobalEventFunctions::luaGlobalEventType] - Invalid type for global event: {}");
                PushBoolean(luaState, false);
            }
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaGlobalEventRegister(LuaState luaState)
    {
        // globalevent:register() 
        var globalevent = GetUserdata<GlobalEvent>(luaState, 1);
        if (globalevent != null)
        {
            if (!globalevent.IsLoadedCallback())
            {
                PushBoolean(luaState, false);
                return 1;
            }
            if (globalevent.EventType == GlobalEventType.GLOBALEVENT_NONE && globalevent.Interval == 0)
            {
                _logger.Error("{} - No interval for globalevent with name {}", nameof(LuaGlobalEventRegister), globalevent.Name);
                PushBoolean(luaState, false);
                return 1;
            }
            PushBoolean(luaState, _globalEvents.RegisterLuaEvent(globalevent));
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaGlobalEventOnCallback(LuaState luaState)
    {
        // globalevent:onThink / record / etc. (callback)
        var globalevent = GetUserdata<GlobalEvent>(luaState, 1);
        if (globalevent != null)
        {
            if (!globalevent.LoadCallback())
            {
                PushBoolean(luaState, false);
                return 1;
            }
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaGlobalEventTime(LuaState luaState)
    {
        // globalevent:time(time)
        var globalevent = GetUserdata<GlobalEvent>(luaState, 1);
        if (globalevent != null)
        {
            string timer = GetString(luaState, 2);

            var parameters = new List<uint>(); // parameters = vectorAtoi(explodeString(timer, ":"));

            foreach (var item in timer.Split(":"))
                parameters.Add(uint.Parse(item));

            var hour = parameters.FirstOrDefault();
            if (hour is < 0 or > 23)
            {
                _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid hour {} for globalevent with name: {}",
                                 timer, globalevent.Name);
                PushBoolean(luaState, false);
                return 1;
            }

            globalevent.Interval = hour << 16;

            uint min = 0;
            uint sec = 0;
            if (parameters.Count > 1)
            {
                min = parameters[1];
                if (min is < 0 or > 59)
                {
                    _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid minute: {} for globalevent with name: {}",
                                     timer, globalevent.Name);
                    PushBoolean(luaState, false);
                    return 1;
                }

                if (parameters.Count > 2)
                {
                    sec = parameters[2];
                    if (sec is < 0 or > 59)
                    {
                        _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid minute: {} for globalevent with name: {}",
                                         timer, globalevent.Name);
                        PushBoolean(luaState, false);
                        return 1;
                    }
                }
            }

            //var current_time = time(nullptr);
            //tm* timeinfo = localtime(&current_time);
            //timeinfo.tm_hour = hour;
            //timeinfo.tm_min = min;
            //timeinfo.tm_sec = sec;

            //time_t difference = static_cast<time_t>(difftime(mktime(timeinfo), current_time));
            //// If the difference is negative, add 86400 seconds (1 day) to it
            //if (difference < 0)
            //{
            //    difference += 86400;
            //}

            DateTime currentTime = DateTime.Now;
            DateTime modifiedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, (int)hour, (int)min, (int)sec);

            TimeSpan difference = modifiedTime - currentTime;

            if (difference < TimeSpan.Zero)
            {
                difference = difference.Add(TimeSpan.FromDays(1));
            }

            int differenceInSeconds = (int)difference.TotalSeconds;

            var test = TimeSpan.FromTicks(currentTime.Ticks) + difference;

            globalevent.NextExecution = (long)test.TotalMilliseconds; ;
            globalevent.EventType = GlobalEventType.GLOBALEVENT_TIMER;
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaGlobalEventInterval(LuaState luaState)
    {
        // globalevent:interval(interval)
        var globalevent = GetUserdata<GlobalEvent>(luaState, 1);
        if (globalevent != null)
        {
            globalevent.Interval = GetNumber<uint>(luaState, 2);
            globalevent.NextExecution = _dispatcher.GlobalTime + GetNumber<uint>(luaState, 2);
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }
}
