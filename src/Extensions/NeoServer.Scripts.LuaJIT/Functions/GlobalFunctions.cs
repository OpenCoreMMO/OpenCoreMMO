using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;
using Serilog;
using System.Threading.Channels;
using System;
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

        #region unsafe scripts   

        //if (g_configManager().getBoolean(WARN_UNSAFE_SCRIPTS) || g_configManager().getBoolean(CONVERT_UNSAFE_SCRIPTS))
        //{
        //    std::vector<std::pair<int32_t, LuaData_t>> indexes;
        //    for (int i = 3; i <= parameters; ++i)
        //    {
        //        if (lua_getmetatable(globalState, i) == 0)
        //        {
        //            continue;
        //        }
        //        lua_rawgeti(luaState, -1, 't');

        //        LuaData_t type = Lua::getNumber<LuaData_t>(luaState, -1);
        //        if (type != LuaData_t::Unknown && type <= LuaData_t::Npc)
        //        {
        //            indexes.emplace_back(i, type);
        //        }
        //        lua_pop(globalState, 2);
        //    }

        //    if (!indexes.empty())
        //    {
        //        if (g_configManager().getBoolean(WARN_UNSAFE_SCRIPTS))
        //        {
        //            bool plural = indexes.size() > 1;

        //            std::string warningString = "Argument";
        //            if (plural)
        //            {
        //                warningString += 's';
        //            }

        //            for (const auto &entry : indexes) {
        //                if (entry == indexes.front())
        //                {
        //                    warningString += ' ';
        //                }
        //                else if (entry == indexes.back())
        //                {
        //                    warningString += " and ";
        //                }
        //                else
        //                {
        //                    warningString += ", ";
        //                }
        //                warningString += '#';
        //                warningString += std::to_string(entry.first);
        //            }

        //            if (plural)
        //            {
        //                warningString += " are unsafe";
        //            }
        //            else
        //            {
        //                warningString += " is unsafe";
        //            }

        //            Lua::reportErrorFunc(warningString);
        //        }

        //        if (g_configManager().getBoolean(CONVERT_UNSAFE_SCRIPTS))
        //        {
        //            for (const auto &entry : indexes) {
        //                switch (entry.second)
        //                {
        //                    case LuaData_t::Item:
        //                    case LuaData_t::Container:
        //                    case LuaData_t::Teleport:
        //                        {
        //                            lua_getglobal(globalState, "Item");
        //                            lua_getfield(globalState, -1, "getUniqueId");
        //                            break;
        //                        }
        //                    case LuaData_t::Player:
        //                    case LuaData_t::Monster:
        //                    case LuaData_t::Npc:
        //                        {
        //                            lua_getglobal(globalState, "Creature");
        //                            lua_getfield(globalState, -1, "getId");
        //                            break;
        //                        }
        //                    default:
        //                        break;
        //                }
        //                lua_replace(globalState, -2);
        //                lua_pushvalue(globalState, entry.first);
        //                lua_call(globalState, 1, 1);
        //                lua_replace(globalState, entry.first);
        //            }
        //        }
        //    }
        //}

        #endregion

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