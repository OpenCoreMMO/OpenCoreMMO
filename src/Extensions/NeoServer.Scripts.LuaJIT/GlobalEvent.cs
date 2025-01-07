using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public struct LightInfo(byte level, byte color)
{
    public byte Level = level;
    public byte Color = color;

    public LightInfo() : this(0, 255)
    {
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

    public override string GetScriptTypeName()
    {
        return EventType switch
        {
            GlobalEventType.GLOBALEVENT_STARTUP => "onStartup",
            GlobalEventType.GLOBALEVENT_SHUTDOWN => "onShutdown",
            GlobalEventType.GLOBALEVENT_RECORD => "onRecord",
            GlobalEventType.GLOBALEVENT_TIMER => "onTime",
            GlobalEventType.GLOBALEVENT_PERIODCHANGE => "onPeriodChange",
            GlobalEventType.GLOBALEVENT_ON_THINK => "onThink", 
            GlobalEventType.GLOBALEVENT_SAVE => "onSave",
            _ => throw new InvalidOperationException("[GlobalEvent::GetScriptTypeName] - Invalid event type")
        };
    }

    public bool ExecutePeriodChange(int lightState, LightInfo lightInfo)
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error("[GlobalEvent::ExecutePeriodChange - {Name}] Call stack overflow. Too many Lua script calls being nested", Name);
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        Lua.PushNumber(luaState, lightState);
        Lua.PushNumber(luaState, lightInfo.Level);
        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteRecord(int current, int old)
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error("[GlobalEvent::ExecuteRecord - {Name}] Call stack overflow. Too many Lua script calls being nested", Name);
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        Lua.PushNumber(luaState, current);
        Lua.PushNumber(luaState, old);
        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteEvent()
    {
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error("[GlobalEvent::ExecuteEvent - {Name}] Call stack overflow. Too many Lua script calls being nested", Name);
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        var paramsCount = 0;
        
        if (EventType is not (GlobalEventType.GLOBALEVENT_NONE or GlobalEventType.GLOBALEVENT_TIMER))
            return scriptInterface.CallFunction(paramsCount);
        
        Lua.PushNumber(luaState, Interval);
        paramsCount = 1;

        return scriptInterface.CallFunction(paramsCount);
    }
}

public class GlobalEventMap : Dictionary<string, GlobalEvent>
{
    // Add any additional functionality or overrides if needed
}