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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "CreatureEvent", "", LuaCreateCreatureEvent);
        RegisterMethod(L, "CreatureEvent", "type", LuaCreatureEventType);
        RegisterMethod(L, "CreatureEvent", "register", LuaCreatureEventRegister);
        RegisterMethod(L, "CreatureEvent", "onLogin", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onLogout", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onThink", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onPrepareDeath", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onDeath", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onKill", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onAdvance", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onModalWindow", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onTextEdit", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onHealthChange", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onManaChange", LuaCreatureEventOnCallback);
        RegisterMethod(L, "CreatureEvent", "onExtendedOpcode", LuaCreatureEventOnCallback);
    }

    private static int LuaCreateCreatureEvent(LuaState L)
    {
        var creatureEvent = new CreatureEvent(GetScriptEnv().GetScriptInterface(), _logger, _scripts);
        creatureEvent.Name = GetString(L, 2);
        PushUserdata(L, creatureEvent);
        SetMetatable(L, -1, "CreatureEvent");
        return 1;
    }

    private static int LuaCreatureEventType(LuaState L)
    {
        // creatureEvent:type(callback)
        var creatureEvent = GetUserdata<CreatureEvent>(L, 1);
        if (creatureEvent != null)
        {
            string typeName = GetString(L, 2);
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
                PushBoolean(L, false);
            }
            creatureEvent.Loaded = true;
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaCreatureEventRegister(LuaState L)
    {
        // creatureEvent:register() 
        var creatureEvent = GetUserdata<CreatureEvent>(L, 1);
        if (creatureEvent != null)
        {
            if (!creatureEvent.IsLoadedScriptId())
            {
                PushBoolean(L, false);
                return 1;
            }
            
            PushBoolean(L, _creatureEvents.RegisterLuaEvent(creatureEvent));
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaCreatureEventOnCallback(LuaState L)
    {
        // creatureevent:onLogin / logout / etc. (callback)
        var creatureEvent = GetUserdata<CreatureEvent>(L, 1);
        if (creatureEvent != null)
        {
            if (!creatureEvent.LoadScriptId())
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
}
