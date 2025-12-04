using LuaNET;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using NeoServer.Scripts.LuaJIT.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Services;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GlobalFunctions : LuaScriptInterface, IGlobalFunctions
{
    private static ILuaEnvironment _luaEnvironment;
    private static ILogger _logger;
    private static IScheduler _scheduler;
    private static IChatChannelStore _chatChannelStore;
    private static IGameServer _gameServer;
    private static HealService _healService;
    private static NonAggressiveCombatService _nonAggressiveCombatService;

    public GlobalFunctions(
        ILuaEnvironment luaEnvironment,
        ILogger logger,
        IScheduler scheduler,
        IChatChannelStore chatChannelStore,
        HealService healService,
        NonAggressiveCombatService nonAggressiveCombatService,
        IGameServer gameServer) : base(nameof(GlobalFunctions))
    {
        _luaEnvironment = luaEnvironment;
        _logger = logger;
        _scheduler = scheduler;
        _chatChannelStore = chatChannelStore;
        _gameServer = gameServer;
        _healService = healService;
        _nonAggressiveCombatService = nonAggressiveCombatService;
    }

    public void Init(LuaState luaState)
    {
        RegisterGlobalMethod(luaState, "rawgetmetatable", LuaRawGetMetatable);
        RegisterGlobalMethod(luaState, "addEvent", LuaAddEvent);
        RegisterGlobalMethod(luaState, "stopEvent", LuaStopEvent);
        RegisterGlobalMethod(luaState, "sendChannelMessage", LuaSendChannelMessage);
        RegisterGlobalMethod(luaState, "getWorldTime", LuaGetWorldTime);
        RegisterGlobalMethod(luaState, "getWorldLight", LuaGetWorldLight);
        RegisterGlobalMethod(luaState, "createCombatArea", LuaCreateCombatArea);
        RegisterGlobalMethod(luaState, "doTargetCombatHealth", LuaDoTargetCombatHealth);
        RegisterGlobalMethod(luaState, "doTargetCombatMana", LuaDoTargetCombatMana);
    }

    private static int HandleCreateCombatFunction(LuaState L)
    {
        return 1;
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

        if (globalState.pointer != luaState.pointer) Lua.XMove(luaState, globalState, Lua.GetTop(luaState));

        var parameters = Lua.GetTop(globalState);
        if (!Lua.IsFunction(globalState, -parameters))
        {
            // -parameters means the first parameter from left to right
            _logger.Error("callback parameter should be a function");
            PushBoolean(luaState, false);
            return 1;
        }

        var eventDesc = new LuaTimerEventDesc();
        for (var i = 0; i < parameters - 2; ++i)
            // -2 because addEvent needs at least two parameters
            eventDesc.Parameters.Add(Lua.Ref(globalState, LUA_REGISTRY_INDEX));

        var delay = int.Max(100, GetNumber<int>(globalState, 2));
        Lua.Pop(globalState, 1);

        eventDesc.Function = Lua.Ref(globalState, LUA_REGISTRY_INDEX);
        eventDesc.ScriptId = GetScriptEnv().GetScriptId();
        eventDesc.ScriptName = GetScriptEnv().GetScriptInterface().GetLoadingScriptName();

        var lastTimerEventId = _luaEnvironment.LastEventTimerId++;

        eventDesc.EventId =
            _scheduler.AddEvent(new SchedulerEvent(delay,
                () => { _luaEnvironment.ExecuteTimerEvent(lastTimerEventId); }));

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

        foreach (var parameter in timerEventDesc.Parameters) Lua.UnRef(globalState, LUA_REGISTRY_INDEX, parameter);

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

    private static int LuaGetWorldTime(LuaState luaState)
    {
        // getWorldTime()
        Lua.PushNumber(luaState, _gameServer.LightHour);
        return 1;
    }

    private static int LuaGetWorldLight(LuaState luaState)
    {
        // getWorldLight()
        Lua.PushNumber(luaState, _gameServer.LightLevel);
        Lua.PushNumber(luaState, _gameServer.LightColor);
        return 2;
    }

    private static int LuaCreateCombatArea(LuaState L)
    {
        // createCombatArea( {area}, <optional> {extArea} )
        return Lua.GetTop(L);
    }

    private static int LuaDoTargetCombatHealth(LuaState luaState)
    {
        // doTargetCombatHealth(cid, target, type, min, max, effect[, origin = ORIGIN_SPELL])
        var creature = GetUserdata<ICreature>(luaState, 1);

        if (creature == null && (!Lua.IsNumber(luaState, 1) || GetNumber<uint>(luaState, 1) != 0))
        {
            _logger.Error("Creature not found");
            PushBoolean(luaState, false);
            return 1;
        }

        var target = GetUserdata<ICreature>(luaState, 2);
        if (target == null)
        {
            _logger.Error("Target creature not found");
            PushBoolean(luaState, false);
            return 1;
        }

        var combatType = GetNumber<CombatType>(luaState, 3);

        var min = GetNumber<int>(luaState, 4);
        var max = GetNumber<int>(luaState, 5);
        var effect = GetNumber<ushort>(luaState, 6);
        var origin = GetNumber<uint>(luaState, 7, 1); // Default to 1 (assuming ORIGIN_SPELL is 1)

        var instantSpellName = GetString(luaState, 9);
        var runeSpellName = GetString(luaState, 10);

        // For simplicity, assume CombatParams and CombatDamage are available
        // This is a placeholder implementation - actual combat logic needs domain integration
        var damageValue = Random.Shared.Next(min, max + 1);

        // Check if it's healing
        var isHealing = combatType == CombatType.COMBAT_HEALING ||
                        (combatType == CombatType.COMBAT_MANADRAIN && damageValue > 0);

        //todo: need to support other parameters

        if (combatType == CombatType.COMBAT_HEALING)
        {
            _healService.Heal(creature, target as ICombatActor, HealType.Health, (ushort)min, (ushort)max);
            EffectService.Send(target.Location, (EffectT)effect);
        }
        // Placeholder for actual combat execution
        // Combat::doCombatHealth(creature, target, damage, params);

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaDoTargetCombatMana(LuaState luaState)
    {
        // doTargetCombatMana(cid, target, min, max, effect[, origin = ORIGIN_SPELL])
        var creature = GetUserdata<ICreature>(luaState, 1);
        if (creature == null && (!Lua.IsNumber(luaState, 1) || GetNumber<uint>(luaState, 1) != 0))
        {
            _logger.Error("Creature not found");
            PushBoolean(luaState, false);
            return 1;
        }

        var target = GetUserdata<ICreature>(luaState, 2);
        if (target == null)
        {
            _logger.Error("Target creature not found");
            PushBoolean(luaState, false);
            return 1;
        }

        var minval = GetNumber<int>(luaState, 3);
        var maxval = GetNumber<int>(luaState, 4);
        var effect = GetNumber<ushort>(luaState, 5);
        var origin = GetNumber<uint>(luaState, 6, 1); // Default to 1 (assuming ORIGIN_SPELL is 1)

        var instantSpellName = GetString(luaState, 7);
        var runeSpellName = GetString(luaState, 8);

        // For simplicity, assume CombatParams and CombatDamage are available
        // This is a placeholder implementation - actual combat logic needs domain integration
        var damageValue = Random.Shared.Next(minval, maxval + 1);

        // Set aggressive based on minval + maxval < 0
        var aggressive = minval + maxval < 0;

        // Placeholder for actual combat execution
        // Combat::doCombatMana(creature, target, damage, params);

        // For now, if it's mana gain (positive), perhaps heal mana
        if (damageValue > 0)
        {
            // Assuming mana gain
            _healService.Heal(creature, target as ICombatActor, HealType.Mana, (ushort)minval, (ushort)maxval);
            EffectService.Send(target.Location, (EffectT)effect);
        }
        else
        {
            // Mana drain - placeholder
            // Need to implement mana drain logic
        }

        PushBoolean(luaState, true);
        return 1;
    }
}