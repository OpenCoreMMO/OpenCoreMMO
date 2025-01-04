using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEvent : Script
{
    private ILogger _logger;
    private IScripts _scripts;

    public CreatureEvent(LuaScriptInterface scriptInterface, ILogger logger, IScripts scripts) : base(scriptInterface)
    {
        _logger = logger;
        _scripts = scripts;
    }

    public string Name { get; set; }
    public CreatureEventType EventType { get; set; }
    public bool Loaded { get; set; } = false;

    public bool LoadScriptId()
    {
        var luaInterface = _scripts.GetScriptInterface();
        SetScriptId(luaInterface.GetEvent());
        if (GetScriptId() == -1)
        {
            _logger.Error("[CreatureEvent::LoadScriptId] Failed to load event. Script name: '{scriptName}', Module: '{moduloeName}'", luaInterface.GetLoadingScriptName(), luaInterface.GetInterfaceName());
            return false;
        }

        return true;
    }

    public bool IsLoadedScriptId() => GetScriptId() != 0;

    public bool ExecuteOnLogin(IPlayer player)
    {
        //onLogin(player)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnLogin - Player {playerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();

        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        return scriptInterface.CallFunction(1);
    }

    public bool ExecuteOnLogout(IPlayer player)
    {
        //onLogout(player)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnLogout - Player {playerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        return scriptInterface.CallFunction(1);
    }

    public bool ExecuteOnThink(ICreature creature, int interval)
    {
        //onThink(creature, interval)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnThink - Creature {creatureName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            creature.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, creature);
        LuaScriptInterface.SetMetatable(L, -1, "Creature");
        Lua.PushNumber(L, interval);

        return scriptInterface.CallFunction(2);
    }

    public bool ExecuteOnPrepareDeath(ICreature creature, ICreature killer, int realDamage)
    {
        //onPrepareDeath(creature, killer)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnPrepareDeath - Creature {creatureName} killer {killerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            creature.Name, killer != null ? killer.Name : string.Empty, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, creature);
        LuaScriptInterface.SetCreatureMetatable(L, -1, creature);

        if (killer is not null)
        {
            LuaScriptInterface.PushUserdata(L, killer);
            LuaScriptInterface.SetCreatureMetatable(L, -1, killer);
        }
        else
        {
            Lua.PushNil(L);
        }

        Lua.PushNumber(L, realDamage);

        return scriptInterface.CallFunction(3);
    }

    public bool ExecuteOnDeath(ICreature creature, IItem corpse, ICreature killer, ICreature mostDamageKiller, bool lastHitUnjustified, bool mostDamageUnjustified)
    {	
        //onDeath(creature, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnDeath - Creature {creatureName} killer {killerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                             creature.Name, killer != null ? killer.Name : string.Empty, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, creature);
        LuaScriptInterface.SetCreatureMetatable(L, -1, creature);

        LuaScriptInterface.PushThing(L, corpse);

        if (killer is not null)
        {
            LuaScriptInterface.PushUserdata(L, killer);
            LuaScriptInterface.SetCreatureMetatable(L, -1, killer);
        }
        else
        {
            Lua.PushNil(L);
        }

        if (mostDamageKiller is not null)
        {
            LuaScriptInterface.PushUserdata(L, mostDamageKiller);
            LuaScriptInterface.SetCreatureMetatable(L, -1, mostDamageKiller);
        }
        else
        {
            Lua.PushNil(L);
        }

        Lua.PushBoolean(L, lastHitUnjustified);
        Lua.PushBoolean(L, mostDamageUnjustified);

        return scriptInterface.CallFunction(6);
    }

    public bool ExecuteOnAdvance(IPlayer player, SkillType skill, int oldLevel, int newLevel)
    {
        //onAdvance(player, skill, oldLevel, newLevel)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnAdvance - Player {playerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                             player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        Lua.PushNumber(L, (int)skill);
        Lua.PushNumber(L, oldLevel);
        Lua.PushNumber(L, newLevel);

        return scriptInterface.CallFunction(4);
    }

    public bool ExecuteOnKill(ICreature creature, ICreature target, bool lastHit)
    {
        // onKill(creature, target, lastHit)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnKill - Creature {creatureName} target {targetName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            creature.Name, target.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, creature);
        LuaScriptInterface.SetCreatureMetatable(L, -1, creature);

        LuaScriptInterface.PushUserdata(L, target);
        LuaScriptInterface.SetCreatureMetatable(L, -1, target);
       
        Lua.PushBoolean(L, lastHit);

        return scriptInterface.CallFunction(3);
    }

    public bool ExecuteOnTextEdit(IPlayer player, IItem item, string text)
    {
        // onTextEdit(player, item, text)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[CreatureEvent::ExecuteOnTextEdit - Player {playerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        LuaScriptInterface.PushThing(L, item);
        LuaScriptInterface.PushString(L, text);

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
            _logger.Error(@"[CreatureEvent::ExecuteOnTextEdit - Player {playerName} event {eventName}] Call stack overflow.
                            Too many lua script calls being nested.",
                            player.Name, Name);

            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        Lua.PushNumber(L, opcode);
        LuaScriptInterface.PushString(L, buffer);

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