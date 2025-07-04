using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Items.Items.UsableItems.Runes;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class LuaRune(LuaScriptInterface scriptInterface) : LuaSpell(scriptInterface)
{
    public int RuneId { get; set; }
    public bool AllowFarUse { get; set; }
    public int Charges { get; set; }
    public bool CheckFloor { get; set; }
    public bool BlockWalls { get; set; }
    public bool CheckLineOfSight { get; set; }
    public ushort ManaConsumption { get; set; }
    public ushort SoulConsumption { get; set; }

    public bool OnUse(ICreature creature, IThing target, Rune rune, bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!GetScriptInterface().InternalReserveScriptEnv())
            // var logger ??= Server.Helpers.IoC.GetInstance<ILogger>();
            //
            // logger.Error(
            //     "[Action::executeUse - Player {PlayerName}, on item {ItemName}]. Call stack overflow. Too many lua script calls being nested. Script name {ScriptName}",
            //     player.Name, item.Name, GetScriptInterface().GetLoadingScriptName());
            return false;

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), scriptInterface);

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaFunctionsLoader.PushUserdata(luaState, creature);
        ;
        LuaFunctionsLoader.SetCreatureMetatable(luaState, -1, creature);


        var variant = new LuaVariant
        {
            Type = LuaVariantType.Number,
            Number = target is ICreature targetCreature ? targetCreature.CreatureId : 0,
            InstantName = "",
            RuneName = rune.Name
        };

        LuaFunctionsLoader.PushVariant(luaState, variant);
        LuaFunctionsLoader.PushBoolean(luaState, isHotkey);

        return GetScriptInterface().CallFunction(3);
    }
}