using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class InstantSpell : ScriptedSpell
{
    public LuaInstantSpell LuaInstantSpell { get; set; }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!LuaInstantSpell.GetScriptInterface().InternalReserveScriptEnv())
            // var logger ??= Server.Helpers.IoC.GetInstance<ILogger>();
            //
            // logger.Error(
            //     "[Action::executeUse - Player {PlayerName}, on item {ItemName}]. Call stack overflow. Too many lua script calls being nested. Script name {ScriptName}",
            //     player.Name, item.Name, GetScriptInterface().GetLoadingScriptName());
            return Result.NotApplicable;

        var scriptInterface = LuaInstantSpell.GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(LuaInstantSpell.GetScriptId(), scriptInterface);

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(LuaInstantSpell.GetScriptId());

        LuaFunctionsLoader.PushUserdata(luaState, caster);

        LuaFunctionsLoader.SetCreatureMetatable(luaState, -1, caster);

        LuaVariant variant = default;

        if (HasParams && Params.Length > 0)
        {
            variant = new LuaVariant
            {
                Type = LuaVariantType.VARIANT_STRING,
                InstantName = LuaInstantSpell.Name,
                Text = Params[0].ToString(),
                RuneName = string.Empty
            };
        }
        else
        {
            var pos = target is IDynamicTile targetTile ? targetTile.Location : caster.Location;

            variant = new LuaVariant
            {
                Type = target is IDynamicTile ? LuaVariantType.VARIANT_POSITION : LuaVariantType.VARIANT_NUMBER,
                Number = target is ICreature targetCreature ? targetCreature.CreatureId : 0,
                Pos = pos,
                InstantName = LuaInstantSpell.Name,
                RuneName = string.Empty
            };
        }

        LuaFunctionsLoader.PushVariant(luaState, variant);
        LuaFunctionsLoader.PushBoolean(luaState, isHotkey);

        return LuaInstantSpell.GetScriptInterface().CallFunction(3) ? Result.Success : Result.NotApplicable;
    }
}