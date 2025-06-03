using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class RuneSpell : ScriptedSpell
{
    public LuaRune LuaRune { get; set; }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!LuaRune.GetScriptInterface().InternalReserveScriptEnv())
            // var logger ??= Server.Helpers.IoC.GetInstance<ILogger>();
            //
            // logger.Error(
            //     "[Action::executeUse - Player {PlayerName}, on item {ItemName}]. Call stack overflow. Too many lua script calls being nested. Script name {ScriptName}",
            //     player.Name, item.Name, GetScriptInterface().GetLoadingScriptName());
            return Result.NotApplicable;

        var scriptInterface = LuaRune.GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(LuaRune.GetScriptId(), scriptInterface);

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(LuaRune.GetScriptId());

        LuaFunctionsLoader.PushUserdata(luaState, caster);
        ;
        LuaFunctionsLoader.SetCreatureMetatable(luaState, -1, caster);

        var variant = new LuaVariant
        {
            Type = LuaVariantType.Number,
            Number = target is ICreature targetCreature ? targetCreature.CreatureId : 0,
            InstantName = "",
            RuneName = LuaRune.Name
        };

        LuaFunctionsLoader.PushVariant(luaState, variant);
        LuaFunctionsLoader.PushBoolean(luaState, isHotkey);

        return LuaRune.GetScriptInterface().CallFunction(3) ? Result.Success : Result.NotApplicable;
    }
}