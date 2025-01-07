using LuaNET;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;
using System.Xml.Linq;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CreatureFunctions : LuaScriptInterface, ICreatureFunctions
{
    private static IGameCreatureManager _gameCreatureManager;
    private static ICreatureEvents _creatureEvents;

    public CreatureFunctions(
        IGameCreatureManager gameCreatureManager,
        ICreatureEvents creatureEvents) : base(nameof(CreatureFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
        _creatureEvents = creatureEvents;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Creature", "", LuaCreatureCreate);
        RegisterMetaMethod(luaState, "Creature", "__eq", LuaUserdataCompare<ICreature>);

        RegisterMethod(luaState, "Creature", "getEvents", LuaCreatureGetEvents);
        RegisterMethod(luaState, "Creature", "registerEvent", LuaCreatureRegisterEvent);
        RegisterMethod(luaState, "Creature", "unregisterEvent", LuaCreatureUnregisterEvent);

        RegisterMethod(luaState, "Creature", "isCreature", LuaCreatureIsCreature);
        RegisterMethod(luaState, "Creature", "isInGhostMode", LuaCreatureIsInGhostMode);
        RegisterMethod(luaState, "Creature", "getId", LuaGetId);
        RegisterMethod(luaState, "Creature", "getName", LuaGetName);
        RegisterMethod(luaState, "Creature", "getPosition", LuaCreatureGetPosition);
        RegisterMethod(luaState, "Creature", "getDirection", LuaCreatureGetDirection);
        RegisterMethod(luaState, "Creature", "say", LuaCreatureSay);
    }

    private static int LuaCreatureCreate(LuaState luaState)
    {
        // Creature(id or name or userdata)

        ICreature creature = null;
        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<int>(luaState, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.CreatureId == id);
        }
        else if (IsString(luaState, 2))
        {
            var name = GetString(luaState, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.Name.Equals(name));
        }
        else if (IsUserdata(luaState, 2))
        {
            var type = GetUserdataType(luaState, 2);
            if (type != LuaDataType.Player && type != LuaDataType.Monster && type != LuaDataType.Npc)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            creature = GetUserdata<ICreature>(luaState, 2);
        }
        else
        {
            creature = null;
        }

        if (creature != null)
        {
            PushUserdata(luaState, creature);
            SetCreatureMetatable(luaState, -1, creature);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaCreatureGetEvents(LuaState luaState)
    {
        // creature:getEvents(type)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (!creature)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var eventType = GetNumber<CreatureEventType>(luaState, 2);
        //var eventList = creature.GetCreatureEvents(eventType);
        var eventList = _creatureEvents.GetCreatureEvents(creature.CreatureId, eventType);

        Lua.CreateTable(luaState, eventList.Count(), 0);

        int index = 0;
        foreach (var creatureEvent in eventList)
        {
            PushString(luaState, creatureEvent.Name);
            Lua.RawSetI(luaState, -2, ++index);
        }
        return 1;
    }

    private static int LuaCreatureRegisterEvent(LuaState luaState)
    {
        // creature:registerEvent(name)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature is not null)
        {
            var eventName = GetString(luaState, 2);
            var creatureEvent = _creatureEvents.GetEventByName(eventName);

            if (creatureEvent is null)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            //PushBoolean(luaState, creature.RegisterCreatureEvent(creatureEvent));
            PushBoolean(luaState, _creatureEvents.RegisterCreatureEvent(creature.CreatureId, creatureEvent));
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaCreatureUnregisterEvent(LuaState luaState)
    {
        // creature:unregisterEvent(name)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature is not null)
        {
            var eventName = GetString(luaState, 2);
            var creatureEvent = _creatureEvents.GetEventByName(eventName);

            if (creatureEvent is null)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            //PushBoolean(luaState, creature.UnregisterCreatureEvent(creatureEvent));
            PushBoolean(luaState, _creatureEvents.UnregisterCreatureEvent(creature.CreatureId, creatureEvent));
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaCreatureIsCreature(LuaState luaState)
    {
        // creature:isCreature()
        Lua.PushBoolean(luaState, GetUserdata<ICreature>(luaState, 1) is not null);
        return 1;
    }

    private static int LuaCreatureIsInGhostMode(LuaState luaState)
    {
        // creature:isInGhostMode()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushBoolean(luaState, creature.IsInvisible);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaGetId(LuaState luaState)
    {
        // creature:getId()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushNumber(luaState, creature.CreatureId);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaGetName(LuaState luaState)
    {
        // creature:getName()
        var creature = GetUserdata<ICreature>(luaState, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            PushBoolean(luaState, false);
            return 0;
        }

        PushString(luaState, creature.Name);
        return 1;
    }

    private static int LuaCreatureGetPosition(LuaState luaState)
    {
        // creature:getPosition()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            PushPosition(luaState, creature.Location);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureGetDirection(LuaState luaState)
    {
        // creature:getDirection()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushNumber(luaState, (byte)creature.Direction);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureSay(LuaState luaState)
    {
        // creature:say(text, type)
        var creature = GetUserdata<ICreature>(luaState, 1);
        var text = GetString(luaState, 2);
        var type = GetNumber<SpeakClassesType>(luaState, 3);

        if (creature != null)
            creature.Say(text, (SpeechType)type);

        PushBoolean(luaState, true);
        return 1;
    }
}