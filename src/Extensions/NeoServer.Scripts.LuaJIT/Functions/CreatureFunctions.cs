using LuaNET;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;

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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Creature", "", LuaCreatureCreate);
        RegisterMetaMethod(L, "Creature", "__eq", LuaUserdataCompare<ICreature>);

        RegisterMethod(L, "Creature", "getEvents", LuaCreatureGetEvents);
        RegisterMethod(L, "Creature", "registerEvent", LuaCreatureRegisterEvent);
        RegisterMethod(L, "Creature", "unregisterEvent", LuaCreatureUnregisterEvent);

        RegisterMethod(L, "Creature", "isCreature", LuaCreatureIsCreature);
        RegisterMethod(L, "Creature", "isInGhostMode", LuaCreatureIsInGhostMode);
        RegisterMethod(L, "Creature", "getId", LuaGetId);
        RegisterMethod(L, "Creature", "getName", LuaGetName);
        RegisterMethod(L, "Creature", "getPosition", LuaCreatureGetPosition);
        RegisterMethod(L, "Creature", "getDirection", LuaCreatureGetDirection);
        RegisterMethod(L, "Creature", "say", LuaCreatureSay);
    }

    private static int LuaCreatureCreate(LuaState L)
    {
        // Creature(id or name or userdata)

        ICreature creature = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<int>(L, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.CreatureId == id);
        }
        else if (IsString(L, 2))
        {
            var name = GetString(L, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.Name.Equals(name));
        }
        else if (IsUserdata(L, 2))
        {
            var type = GetUserdataType(L, 2);
            if (type != LuaDataType.Player && type != LuaDataType.Monster && type != LuaDataType.Npc)
            {
                Lua.PushNil(L);
                return 1;
            }

            creature = GetUserdata<ICreature>(L, 2);
        }
        else
        {
            creature = null;
        }

        if (creature != null)
        {
            PushUserdata(L, creature);
            SetCreatureMetatable(L, -1, creature);
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    private static int LuaCreatureGetEvents(LuaState L)
    {
        // creature:getEvents(type)
        var creature = GetUserdata<ICreature>(L, 1);
        if (!creature)
        {
            Lua.PushNil(L);
            return 1;
        }

        var eventType = GetNumber<CreatureEventType>(L, 2);
        //var eventList = creature.GetCreatureEvents(eventType);
        var eventList = _creatureEvents.GetCreatureEvents(creature.CreatureId, eventType);

        Lua.CreateTable(L, eventList.Count(), 0);

        int index = 0;
        foreach (var creatureEvent in eventList)
        {
            PushString(L, creatureEvent.Name);
            Lua.RawSetI(L, -2, ++index);
        }
        return 1;
    }

    private static int LuaCreatureRegisterEvent(LuaState L)
    {
        // creature:registerEvent(name)
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature is not null)
        {
            var eventName = GetString(L, 2);
            var creatureEvent = _creatureEvents.GetEventByName(eventName);

            if (creatureEvent is null)
            {
                Lua.PushNil(L);
                return 1;
            }

            //PushBoolean(L, creature.RegisterCreatureEvent(creatureEvent));
            PushBoolean(L, _creatureEvents.RegisterCreatureEvent(creature.CreatureId, creatureEvent));
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    private static int LuaCreatureUnregisterEvent(LuaState L)
    {
        // creature:unregisterEvent(name)
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature is not null)
        {
            var eventName = GetString(L, 2);
            var creatureEvent = _creatureEvents.GetEventByName(eventName);

            if (creatureEvent is null)
            {
                Lua.PushNil(L);
                return 1;
            }

            //PushBoolean(L, creature.UnregisterCreatureEvent(creatureEvent));
            PushBoolean(L, _creatureEvents.UnregisterCreatureEvent(creature.CreatureId, creatureEvent));
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    private static int LuaCreatureIsCreature(LuaState L)
    {
        // creature:isCreature()
        Lua.PushBoolean(L, GetUserdata<ICreature>(L, 1) is not null);
        return 1;
    }

    private static int LuaCreatureIsInGhostMode(LuaState L)
    {
        // creature:isInGhostMode()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            Lua.PushBoolean(L, creature.IsInvisible);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaGetId(LuaState L)
    {
        // creature:getId()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            Lua.PushNumber(L, creature.CreatureId);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaGetName(LuaState L)
    {
        // creature:getName()
        var creature = GetUserdata<ICreature>(L, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            PushBoolean(L, false);
            return 0;
        }

        PushString(L, creature.Name);
        return 1;
    }

    private static int LuaCreatureGetPosition(LuaState L)
    {
        // creature:getPosition()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            PushPosition(L, creature.Location);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaCreatureGetDirection(LuaState L)
    {
        // creature:getDirection()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            Lua.PushNumber(L, (byte)creature.Direction);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaCreatureSay(LuaState L)
    {
        // creature:say(text, type)
        var creature = GetUserdata<ICreature>(L, 1);
        var text = GetString(L, 2);
        var type = GetNumber<SpeakClassesType>(L, 3);

        if (creature != null)
            creature.Say(text, (SpeechType)type);

        PushBoolean(L, true);
        return 1;
    }
}