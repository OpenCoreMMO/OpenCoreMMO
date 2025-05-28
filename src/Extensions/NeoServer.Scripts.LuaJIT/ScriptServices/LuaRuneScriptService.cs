using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Scripts.LuaJIT.DataManagers;
using NeoServer.Server.Common.Contracts.Scripts.Services;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaRuneScriptService(RuneManager runeManager) : IRuneScriptService
{
    public bool HasScript(IAttackRune rune) => runeManager.IsRegistered(rune.ClientId);

    public bool UseItem(IPlayer player, IThing target, IRune rune, bool isHotkey)
    {
        // if (!runeManager.IsRegistered(item.ClientId))
        // {
        //     return false;
        // }
        //
        // if(item is not IRune rune)
        // {
        //     return false;
        // }
        //
        // if (target != null)
        // {
        //     if (target is ITile tile)
        //     {
        //         target = tile.TopItemOnStack;
        //     }
        //     else if (target is ICreature creature)
        //     {
        //         toPos = creature.Location;
        //         toStackPos = creature.Tile.GetCreatureStackPositionIndex(player);
        //     }
        // }

        var luaRune = runeManager.GetRegisteredRune(rune.ClientId);

        luaRune?.OnUse(player, target, rune, isHotkey);

        return false;
    }
}