using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CreatureEventFunctions : LuaScriptInterface, ICreatureEventFunctions
{
    private static ILogger _logger;
    private static ICreatureEvents _creatureEvents;
    private static IScripts _scripts;

    public CreatureEventFunctions(
        ILogger logger,
        ICreatureEvents creatureEvents,
        IScripts scripts) : base(nameof(CreatureEventFunctions))

    {
        _logger = logger;
        _creatureEvents = creatureEvents;
        _scripts = scripts;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "CreatureEvent", "", LuaCreateCreatureEvent);
        RegisterMethod(luaState, "CreatureEvent", "type", LuaCreatureEventType);
        RegisterMethod(luaState, "CreatureEvent", "register", LuaCreatureEventRegister);
        RegisterMethod(luaState, "CreatureEvent", "onLogin", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onLogout", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onThink", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onPrepareDeath", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onDeath", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onKill", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onAdvance", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onModalWindow", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onTextEdit", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onHealthChange", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onManaChange", LuaCreatureEventOnCallback);
        RegisterMethod(luaState, "CreatureEvent", "onExtendedOpcode", LuaCreatureEventOnCallback);
    }

    private static int LuaCreateCreatureEvent(LuaState luaState)
    {
        var creatureEvent = new CreatureEvent(GetScriptEnv().GetScriptInterface(), _logger, _scripts);
        creatureEvent.Name = GetString(luaState, 2);
        PushUserdata(luaState, creatureEvent);
        SetMetatable(luaState, -1, "CreatureEvent");
        return 1;
    }

    private static int LuaCreatureEventType(LuaState luaState)
    {
        // creatureEvent:type(callback)
        var creatureEvent = GetUserdata<CreatureEvent>(luaState, 1);
        if (creatureEvent != null)
        {
            string typeName = GetString(luaState, 2);
            string tmpStr = typeName.ToLower();
            if (tmpStr == "login")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_LOGIN;
            }
            else if (tmpStr == "logout")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_LOGOUT;
            }
            else if (tmpStr == "think")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_THINK;
            }
            else if (tmpStr == "preparedeath")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_PREPAREDEATH;
            }
            else if (tmpStr == "death")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_DEATH;
            }
            else if (tmpStr == "kill")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_KILL;
            }
            else if (tmpStr == "advance")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_ADVANCE;
            }
            else if (tmpStr == "modalwindow")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_MODALWINDOW;
            }
            else if (tmpStr == "textedit")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_TEXTEDIT;
            }
            else if (tmpStr == "healthchange")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_HEALTHCHANGE;
            }
            else if (tmpStr == "manachange")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_MANACHANGE;
            }
            else if (tmpStr == "extendedopcode")
            {
                creatureEvent.EventType = CreatureEventType.CREATURE_EVENT_EXTENDED_OPCODE;
            }
            else
            {
                _logger.Error("[CreatureEventFunctions::LuaCreatureEventType] - Invalid type for creature event: {}");
                PushBoolean(luaState, false);
            }
            creatureEvent.Loaded = true;
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaCreatureEventRegister(LuaState luaState)
    {
        // creatureEvent:register() 
        var creatureEvent = GetUserdata<CreatureEvent>(luaState, 1);
        if (creatureEvent != null)
        {
            if (!creatureEvent.IsLoadedScriptId())
            {
                PushBoolean(luaState, false);
                return 1;
            }
            
            PushBoolean(luaState, _creatureEvents.RegisterLuaEvent(creatureEvent));
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaCreatureEventOnCallback(LuaState luaState)
    {
        // creatureevent:onLogin / logout / etc. (callback)
        var creatureEvent = GetUserdata<CreatureEvent>(luaState, 1);
        if (creatureEvent != null)
        {
            if (!creatureEvent.LoadScriptId())
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
}
