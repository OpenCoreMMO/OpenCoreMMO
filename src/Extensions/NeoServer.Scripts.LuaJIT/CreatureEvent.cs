using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEvent(LuaScriptInterface scriptInterface, ILogger logger, IScripts scripts)
    : Script(scriptInterface)
{
    public string Name { get; set; }
    public CreatureEventType EventType { get; set; }
    public bool Loaded { get; set; }

    public bool LoadScriptId()
    {
        var luaInterface = scripts.GetScriptInterface();
        SetScriptId(luaInterface.GetEvent());
        if (GetScriptId() != -1) return true;
        
        logger.Error("[CreatureEvent::LoadScriptId] Failed to load event. Script name: '{ScriptName}', Module: '{ModuleName}'", luaInterface.GetLoadingScriptName(), luaInterface.GetInterfaceName());
        return false;
    }

    public bool IsLoadedScriptId() => GetScriptId() != 0;

    public bool ExecuteOnLogin(IPlayer player)
    {
        //onLogin(player)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnLogin - Player {PlayerName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();

        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        return scriptInterface.CallFunction(1);
    }

    public bool ExecuteOnLogout(IPlayer player)
    {
        //onLogout(player)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnLogout - Player {PlayerName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        return scriptInterface.CallFunction(1);
    }

    public bool ExecuteOnThink(ICreature creature, int interval)
    {
        //onThink(creature, interval)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnThink - Creature {CreatureName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            creature.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, creature);
        LuaScriptInterface.SetMetatable(luaState, -1, "Creature");
        Lua.PushNumber(luaState, interval);

        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteOnPrepareDeath(ICreature creature, ICreature killer, int realDamage)
    {
        //onPrepareDeath(creature, killer)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnPrepareDeath - Creature {creatureName} killer {killerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            creature.Name, killer != null ? killer.Name : string.Empty, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, creature);
        LuaScriptInterface.SetCreatureMetatable(luaState, -1, creature);

        if (killer is not null)
        {
            LuaScriptInterface.PushUserdata(luaState, killer);
            LuaScriptInterface.SetCreatureMetatable(luaState, -1, killer);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        Lua.PushNumber(luaState, realDamage);

        return scriptInterface.CallFunction(3);
    }

    public bool ExecuteOnDeath(ICreature creature, IItem corpse, ICreature killer, ICreature mostDamageKiller, bool lastHitUnjustified, bool mostDamageUnjustified)
    {	
        //onDeath(creature, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnDeath - Creature {creatureName} killer {killerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                             creature.Name, killer != null ? killer.Name : string.Empty, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, creature);
        LuaScriptInterface.SetCreatureMetatable(luaState, -1, creature);

        LuaScriptInterface.PushThing(luaState, corpse);

        if (killer is not null)
        {
            LuaScriptInterface.PushUserdata(luaState, killer);
            LuaScriptInterface.SetCreatureMetatable(luaState, -1, killer);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        if (mostDamageKiller is not null)
        {
            LuaScriptInterface.PushUserdata(luaState, mostDamageKiller);
            LuaScriptInterface.SetCreatureMetatable(luaState, -1, mostDamageKiller);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        Lua.PushBoolean(luaState, lastHitUnjustified);
        Lua.PushBoolean(luaState, mostDamageUnjustified);

        return scriptInterface.CallFunction(6);
    }

    public bool ExecuteOnAdvance(IPlayer player, SkillType skill, int oldLevel, int newLevel)
    {
        //onAdvance(player, skill, oldLevel, newLevel)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnAdvance - Player {PlayerName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                             player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        Lua.PushNumber(luaState, (int)skill);
        Lua.PushNumber(luaState, oldLevel);
        Lua.PushNumber(luaState, newLevel);

        return scriptInterface.CallFunction(4);
    }

    public bool ExecuteOnKill(ICreature creature, ICreature target, bool lastHit)
    {
        // onKill(creature, target, lastHit)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnKill - Creature {CreatureName} target {TargetName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            creature.Name, target.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, creature);
        LuaScriptInterface.SetCreatureMetatable(luaState, -1, creature);

        LuaScriptInterface.PushUserdata(luaState, target);
        LuaScriptInterface.SetCreatureMetatable(luaState, -1, target);
       
        Lua.PushBoolean(luaState, lastHit);

        return scriptInterface.CallFunction(3);
    }

    public bool ExecuteOnTextEdit(IPlayer player, IItem item, string text)
    {
        // onTextEdit(player, item, text)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error(@"[CreatureEvent::ExecuteOnTextEdit - Player {PlayerName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        LuaScriptInterface.PushThing(luaState, item);
        LuaScriptInterface.PushString(luaState, text);

        return scriptInterface.CallFunction(3);
    }

    //todo: implement this
    //public bool ExecuteOnHealthChange(ICreature creature, ICreature attacker, CombatDamage damage)) { }

    //todo: implement this
    //public bool ExecuteOnManaChange(ICreature creature, ICreature attacker, CombatDamage damage)) { }

    public bool ExecuteOnExtendedOpcode(IPlayer player, byte opcode, string buffer)
    {
        // onExtendedOpcode(player, opcode, buffer)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            logger.Error("[CreatureEvent::ExecuteOnTextEdit - Player {PlayerName} event {EventName}] Call stack overflow, too many lua script calls being nested",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        Lua.PushNumber(luaState, opcode);
        LuaScriptInterface.PushString(luaState, buffer);

        return scriptInterface.CallFunction(3);
    }

    public void CopyEvent(CreatureEvent creatureEvent)
    {
        SetScriptId(creatureEvent.GetScriptId());
        Loaded = creatureEvent.Loaded;
    }

    public void ClearEvent()
    {
        SetScriptId(0);
        Loaded = false;
    }
}