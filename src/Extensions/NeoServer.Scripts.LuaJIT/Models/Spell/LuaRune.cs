using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class LuaRune(LuaScriptInterface scriptInterface) : LuaSpell(scriptInterface)
{
    public int RuneId { get; set; }
    public bool AllowFarUse { get; set; }
    public int Charges { get; set; }
    public bool CheckFloor { get; set; }
    public bool BlockWalls { get; set; }

    public bool OnUse(ICreature player, IAttackRune rune, bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            // var logger ??= Server.Helpers.IoC.GetInstance<ILogger>();
            //
            // logger.Error(
            //     "[Action::executeUse - Player {PlayerName}, on item {ItemName}]. Call stack overflow. Too many lua script calls being nested. Script name {ScriptName}",
            //     player.Name, item.Name, GetScriptInterface().GetLoadingScriptName());
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Creature");

         LuaScriptInterface.PushThing(luaState, rune);
        // LuaScriptInterface.PushPosition(luaState, fromPosition);
        //
        // LuaScriptInterface.PushThing(luaState, target);
        // LuaScriptInterface.PushPosition(luaState, toPosition);
        //
        LuaScriptInterface.PushBoolean(luaState, isHotkey);

        return GetScriptInterface().CallFunction(3);
    }
}