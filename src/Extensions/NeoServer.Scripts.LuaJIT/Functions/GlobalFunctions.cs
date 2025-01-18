using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;
using Serilog;
using NeoServer.Game.Common.Chats;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GlobalFunctions : LuaScriptInterface, IGlobalFunctions
{
    private static ILuaEnvironment _luaEnvironment;
    private static ILogger _logger;
    private static IScheduler _scheduler;
    private static IChatChannelStore _chatChannelStore;

    public GlobalFunctions(
        ILuaEnvironment luaEnvironment,
        ILogger logger, 
        IScheduler scheduler,
        IChatChannelStore chatChannelStore) : base(nameof(GlobalFunctions))
    {
        _luaEnvironment = luaEnvironment;
        _logger = logger;
        _scheduler = scheduler;
        _chatChannelStore = chatChannelStore;
    }

    public void Init(LuaState luaState)
    {
        RegisterGlobalMethod(luaState, "rawgetmetatable", LuaRawGetMetatable);
        RegisterGlobalMethod(luaState, "addEvent", LuaAddEvent);
        RegisterGlobalMethod(luaState, "stopEvent", LuaStopEvent);
        RegisterGlobalMethod(luaState, "sendChannelMessage", LuaSendChannelMessage);
    }

    private static int LuaRawGetMetatable(LuaState luaState)
    {
        // rawgetmetatable(metatableName)
        Lua.GetMetaTable(luaState, GetString(luaState, 1));
        return 1;
    }

    private static int LuaAddEvent(LuaState luaState)
    {
        // addEvent(callback, delay, ...)
        var globalState = _luaEnvironment.GetLuaState();
        if (globalState.IsNull)
        {
            _logger.Error("No valid script interface!");
            PushBoolean(luaState, false);
            return 1;
        }
        else if (globalState.pointer != luaState.pointer)
        {
            Lua.XMove(luaState, globalState, Lua.GetTop(luaState));
        }

        int parameters = Lua.GetTop(globalState);
        if (!Lua.IsFunction(globalState, -parameters))
        { 
            // -parameters means the first parameter from left to right
            _logger.Error("callback parameter should be a function.");
            PushBoolean(luaState, false);
            return 1;
        }

        LuaTimerEventDesc eventDesc = new LuaTimerEventDesc();
        for (int i = 0; i < parameters - 2; ++i)
        { 
            // -2 because addEvent needs at least two parameters
            eventDesc.Parameters.Add(Lua.Ref(globalState, LUA_REGISTRY_INDEX));
        }

        var delay = int.Max(100, GetNumber<int>(globalState, 2));
        Lua.Pop(globalState, 1);

        eventDesc.Function = Lua.Ref(globalState, LUA_REGISTRY_INDEX);
        eventDesc.ScriptId = GetScriptEnv().GetScriptId();
        eventDesc.ScriptName = GetScriptEnv().GetScriptInterface().GetLoadingScriptName();

        var lastTimerEventId = _luaEnvironment.LastEventTimerId++;

        eventDesc.EventId = _scheduler.AddEvent(new SchedulerEvent(delay, () =>
        {
            _luaEnvironment.ExecuteTimerEvent(lastTimerEventId);
        }));

        _luaEnvironment.TimerEvents.Add(lastTimerEventId, eventDesc);

        Lua.PushNumber(luaState, lastTimerEventId);
        return 1;
    }

    private static int LuaStopEvent(LuaState luaState)
    {
        // stopEvent(eventid)
        var globalState = _luaEnvironment.GetLuaState();
        if (globalState.IsNull)
        {
            _logger.Error("No valid script interface!");
            PushBoolean(luaState, false);
            return 1;
        }

        var eventId = GetNumber<uint>(luaState, 1);

        if (!_luaEnvironment.TimerEvents.TryGetValue(eventId, out var timerEventDesc))
        {
            PushBoolean(luaState, false);
            return 1;
        }

        _luaEnvironment.TimerEvents.Remove(eventId);

        _scheduler.CancelEvent(timerEventDesc.EventId);

        Lua.UnRef(globalState, LUA_REGISTRY_INDEX, timerEventDesc.Function);

        foreach (var parameter in timerEventDesc.Parameters) {
            Lua.UnRef(globalState, LUA_REGISTRY_INDEX, parameter);
        }

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaSendChannelMessage(LuaState luaState)
    {
        // sendChannelMessage(channelId, type, message)
        var globalState = _luaEnvironment.GetLuaState();

        var channelId = GetNumber<ushort>(luaState, 1);
        if (!_chatChannelStore.TryGetValue(channelId, out var channel) || channel is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        var type = GetNumber<SpeakClassesType>(luaState, 2);
        var message = GetString(luaState, 3);

        channel.WriteMessage(message, out var cancelMessage, (SpeechType)type);
        PushBoolean(luaState, true);
        return 1;
    }
}