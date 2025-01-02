using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public struct LightInfo
{
    public byte Level;
    public byte Color;

    public LightInfo()
    {
        Level = 0;
        Color = 255;
    }

    public LightInfo(byte level, byte color)
    {
        Level = level;
        Color = color;
    }
}

public class GlobalEvent : Script
{
    private ILogger _logger;

    public GlobalEvent(LuaScriptInterface scriptInterface, ILogger logger) : base(scriptInterface)
    {
        _logger = logger;
    }

    public string Name { get; set; }
    public long NextExecution { get; set; }
    public uint Interval { get; set; }
    public GlobalEventType EventType { get; set; }

    public string GetScriptTypeName()
    {
        switch (EventType)
        {
            case GlobalEventType.GLOBALEVENT_STARTUP:
                return "onStartup";
            case GlobalEventType.GLOBALEVENT_SHUTDOWN:
                return "onShutdown";
            case GlobalEventType.GLOBALEVENT_RECORD:
                return "onRecord";
            case GlobalEventType.GLOBALEVENT_TIMER:
                return "onTime";
            case GlobalEventType.GLOBALEVENT_PERIODCHANGE:
                return "onPeriodChange";
            case GlobalEventType.GLOBALEVENT_ON_THINK:
                return "onThink";
            default:
                throw new InvalidOperationException("[GlobalEvent::GetScriptTypeName] - Invalid event type");
        }
    }

    public bool ExecutePeriodChange(int lightState, LightInfo lightInfo)
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error($"[GlobalEvent::ExecutePeriodChange - {Name}] Call stack overflow. Too many Lua script calls being nested.");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        Lua.PushNumber(L, lightState);
        Lua.PushNumber(L, lightInfo.Level);
        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteRecord(int current, int old)
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error($"[GlobalEvent::ExecuteRecord - {Name}] Call stack overflow. Too many Lua script calls being nested.");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        Lua.PushNumber(L, current);
        Lua.PushNumber(L, old);
        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteEvent()
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error($"[GlobalEvent::ExecuteEvent - {Name}] Call stack overflow. Too many Lua script calls being nested.");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        int paramsCount = 0;
        if (EventType == GlobalEventType.GLOBALEVENT_NONE || EventType == GlobalEventType.GLOBALEVENT_TIMER)
        {
            Lua.PushNumber(L, Interval);
            paramsCount = 1;
        }

        return scriptInterface.CallFunction(paramsCount);
    }
}

public class GlobalEventMap : Dictionary<string, GlobalEvent>
{
    // Add any additional functionality or overrides if needed
}