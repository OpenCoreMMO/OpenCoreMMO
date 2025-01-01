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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "GlobalEvent", "", LuaCreateGlobalEvent);
        RegisterMethod(L, "GlobalEvent", "type", LuaGlobalEventType);
        RegisterMethod(L, "GlobalEvent", "register", LuaGlobalEventRegister);
        RegisterMethod(L, "GlobalEvent", "time", LuaGlobalEventTime);
        RegisterMethod(L, "GlobalEvent", "interval", LuaGlobalEventInterval);
        RegisterMethod(L, "GlobalEvent", "onThink", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onTime", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onStartup", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onShutdown", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onRecord", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onPeriodChange", LuaGlobalEventOnCallback);
        RegisterMethod(L, "GlobalEvent", "onSave", LuaGlobalEventOnCallback);
    }

    private static int LuaCreateGlobalEvent(LuaState L)
    {
        var global = new GlobalEvent(GetScriptEnv().GetScriptInterface(), _logger);
        global.Name = GetString(L, 2);
        global.EventType = GlobalEventType.GLOBALEVENT_NONE;
        PushUserdata(L, global);
        SetMetatable(L, -1, "GlobalEvent");
        return 1;
    }

    private static int LuaGlobalEventType(LuaState L)
    {
        // globalevent:type(callback)
        var global = GetUserdata<GlobalEvent>(L, 1);
        if (global != null)
        {
            string typeName = GetString(L, 2);
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
            else
            {
                _logger.Error("[GlobalEventFunctions::luaGlobalEventType] - Invalid type for global event: {}");
                PushBoolean(L, false);
            }
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGlobalEventRegister(LuaState L)
    {
        // globalevent:register() 
        var globalevent = GetUserdata<GlobalEvent>(L, 1);
        if (globalevent != null)
        {
            if (!globalevent.IsLoadedCallback())
            {
                PushBoolean(L, false);
                return 1;
            }
            if (globalevent.EventType == GlobalEventType.GLOBALEVENT_NONE && globalevent.Interval == 0)
            {
                _logger.Error("{} - No interval for globalevent with name {}", nameof(LuaGlobalEventRegister), globalevent.Name);
                PushBoolean(L, false);
                return 1;
            }
            PushBoolean(L, _globalEvents.RegisterLuaEvent(globalevent));
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGlobalEventOnCallback(LuaState L)
    {
        // globalevent:onThink / record / etc. (callback)
        var globalevent = GetUserdata<GlobalEvent>(L, 1);
        if (globalevent != null)
        {
            if (!globalevent.LoadCallback())
            {
                PushBoolean(L, false);
                return 1;
            }
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGlobalEventTime(LuaState L)
    {
        // globalevent:time(time)
        var globalevent = GetUserdata<GlobalEvent>(L, 1);
        if (globalevent != null)
        {
            string timer = GetString(L, 2);

            var parameters = new List<uint>(); // parameters = vectorAtoi(explodeString(timer, ":"));

            foreach (var item in timer.Split(":"))
                parameters.Add(uint.Parse(item));

            var hour = parameters.FirstOrDefault();
            if (hour < 0 || hour > 23)
            {
                _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid hour {} for globalevent with name: {}",
                                 timer, globalevent.Name);
                PushBoolean(L, false);
                return 1;
            }

            globalevent.Interval = hour << 16;

            uint min = 0;
            uint sec = 0;
            if (parameters.Count > 1)
            {
                min = parameters[1];
                if (min < 0 || min > 59)
                {
                    _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid minute: {} for globalevent with name: {}",
                                     timer, globalevent.Name);
                    PushBoolean(L, false);
                    return 1;
                }

                if (parameters.Count > 2)
                {
                    sec = parameters[2];
                    if (sec < 0 || sec > 59)
                    {
                        _logger.Error("[GlobalEventFunctions::luaGlobalEventTime] - Invalid minute: {} for globalevent with name: {}",
                                         timer, globalevent.Name);
                        PushBoolean(L, false);
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
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGlobalEventInterval(LuaState L)
    {
        // globalevent:interval(interval)
        var globalevent = GetUserdata<GlobalEvent>(L, 1);
        if (globalevent != null)
        {
            globalevent.Interval = GetNumber<uint>(L, 2);
            globalevent.NextExecution = _dispatcher.GlobalTime + GetNumber<uint>(L, 2);
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }
}
