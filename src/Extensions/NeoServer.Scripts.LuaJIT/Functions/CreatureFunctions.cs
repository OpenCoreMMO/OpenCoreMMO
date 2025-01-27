using LuaNET;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Events.Creature;
using System;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CreatureFunctions : LuaScriptInterface, ICreatureFunctions
{
    private static IGameCreatureManager _gameCreatureManager;
    private static ICreatureEvents _creatureEvents;
    private static CreatureHealedEventHandler _creatureHealedEventHandler;

    public CreatureFunctions(
        IGameCreatureManager gameCreatureManager,
        ICreatureEvents creatureEvents,
        CreatureHealedEventHandler creatureHealedEventHandler) : base(nameof(CreatureFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
        _creatureEvents = creatureEvents;
        _creatureHealedEventHandler = creatureHealedEventHandler;
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
        RegisterMethod(luaState, "Creature", "getId", LuaCreatureGetId);
        RegisterMethod(luaState, "Creature", "getName", LuaCreatureGetName);
        RegisterMethod(luaState, "Creature", "getPosition", LuaCreatureGetPosition);
        RegisterMethod(luaState, "Creature", "getDirection", LuaCreatureGetDirection);

        RegisterMethod(luaState, "Creature", "getHealth", LuaCreatureGetHealth);
        RegisterMethod(luaState, "Creature", "setHealth", LuaCreatureSetHealth);
        RegisterMethod(luaState, "Creature", "addHealth", LuaCreatureAddHealth);
        RegisterMethod(luaState, "Creature", "getMaxHealth", LuaCreatureGetMaxHealth);
        RegisterMethod(luaState, "Creature", "setMaxHealth", LuaCreatureSetMaxHealth);
        RegisterMethod(luaState, "Creature", "setHiddenHealth", LuaCreatureSetHiddenHealth);

        RegisterMethod(luaState, "Creature", "getCondition", LuaCreatureGetCondition);
        RegisterMethod(luaState, "Creature", "addCondition", LuaCreatureAddCondition);
        RegisterMethod(luaState, "Creature", "removeCondition", LuaCreatureRemoveCondition);
        RegisterMethod(luaState, "Creature", "hasCondition", LuaCreatureHasCondition);

        RegisterMethod(luaState, "Creature", "say", LuaCreatureSay);

        RegisterMethod(luaState, "Creature", "getSummons", LuaCreatureGetSummons);
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

    private static int LuaCreatureGetId(LuaState luaState)
    {
        // creature:getId()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushNumber(luaState, creature.CreatureId);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureGetName(LuaState luaState)
    {
        // creature:getName()
        var creature = GetUserdata<ICreature>(luaState, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaCreatureGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
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

    private static int LuaCreatureGetHealth(LuaState luaState)
    {
        // creature:getHealth()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushNumber(luaState, creature.HealthPoints);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureSetHealth(LuaState luaState)
    {
        // creature:setHealth(health)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (!creature)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var health = GetNumber<uint>(luaState, 2);
        creature.HealthPoints = Math.Min(health, creature.MaxHealthPoints);

        _creatureHealedEventHandler.Execute(creature, null, 0);

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaCreatureAddHealth(LuaState luaState)
    {
        // creature:addHealth(healthChange, combatType)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (!creature || creature is not ICombatActor combatActor)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var healthChange = GetNumber<ushort>(luaState, 2);
        combatActor.Heal(healthChange, null);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaCreatureGetMaxHealth(LuaState luaState)
    {
        // creature:getMaxHealth()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null)
            Lua.PushNumber(luaState, creature.MaxHealthPoints);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureSetMaxHealth(LuaState luaState)
    {
        // creature:setMaxHealth(maxHealth)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (!creature)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var maxHealth = GetNumber<uint>(luaState, 2);

        creature.HealthPoints = maxHealth;
        creature.HealthPoints = Math.Min(creature.HealthPoints, creature.MaxHealthPoints);

        _creatureHealedEventHandler.Execute(creature, null, 0);

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaCreatureSetHiddenHealth(LuaState luaState)
    {
        // creature:setHiddenHealth(hide)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature is not null)
        {
            creature.IsHealthHidden = GetBoolean(luaState, 2);
            _creatureHealedEventHandler.Execute(creature, null, 0);
            Lua.PushBoolean(luaState, true);
            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureGetCondition(LuaState luaState)
    {
        // creature:getCondition(conditionType, conditionId = CONDITIONID_COMBAT, subId = 0)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null && creature is ICombatActor combatActor)
        {
            var conditionType = GetNumber<ConditionType>(luaState, 2);
            var condition = combatActor.GetCondition(conditionType);
            if (condition != null) 
            {
                PushUserdata(luaState, condition);
                SetWeakMetatable(luaState, -1, "Condition");
            }
        }
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureAddCondition(LuaState luaState)
    {
        // creature:addCondition(conditionType)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null && creature is ICombatActor combatActor)
        {
            var condition = GetUserdata<ICondition>(luaState, 2);
            combatActor.AddCondition(condition);
            Lua.PushBoolean(luaState, true);
        }
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureHasCondition(LuaState luaState)
    {
        // creature:hasCondition(conditionType)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null && creature is ICombatActor combatActor)
        {
            var conditionType = GetNumber<ConditionType>(luaState, 2);
            Lua.PushBoolean(luaState, combatActor.HasCondition(conditionType));
        }
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureRemoveCondition(LuaState luaState)
    {
        // creature:removeCondition(conditionType)
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature != null && creature is ICombatActor combatActor)
        {
            var conditionType = GetNumber<ConditionType>(luaState, 2);
            combatActor.RemoveCondition(conditionType);
            Lua.PushBoolean(luaState, true);
        }
        else
            Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaCreatureSay(LuaState luaState)
    {
        // creature:say(text, type, ghost = false, target = nullptr, position)

        var parameters = Lua.GetTop(luaState);

        var creature = GetUserdata<ICreature>(luaState, 1);
        var text = GetString(luaState, 2);
        var type = GetNumber<SpeakClassesType>(luaState, 3);
        var ghost = GetBoolean(luaState, 4, false);

        ICreature target = null;
        if(parameters >= 5)
            target = GetUserdata<ICreature>(luaState, 5);

        Location position;
        if (parameters >= 6)
            position = GetUserdataStruct<Location>(luaState, 6);

        if (creature != null)
        {
            creature.Say(text, (SpeechType)type, target);
            PushBoolean(luaState, true);
            return 1;
        }

        PushBoolean(luaState, false);
        return 1;
    }

    private static int LuaCreatureGetSummons(LuaState luaState)
    {
        // creature:getSummons()
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (!creature)
        {
            ReportError(nameof(LuaCreatureGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        var summons = creature.Summons;
        Lua.CreateTable(luaState, summons.Count, 0);

        var index = 0;
        foreach (var summon in summons)
        {
            PushThing(luaState, summon);
            Lua.RawSetI(luaState, -2, ++index);
        }

        return 1;
    }
}