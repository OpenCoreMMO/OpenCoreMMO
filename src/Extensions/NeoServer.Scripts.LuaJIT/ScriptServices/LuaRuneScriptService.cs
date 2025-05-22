using System.Text;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.DataManagers;
using NeoServer.Scripts.LuaJIT.Models.Spell;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaRuneScriptService(ILogger logger, RuneManager runeManager) : IRuneScriptService
{
    public bool UseItem(IPlayer player, IAttackRune rune, bool isHotkey)
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
        if (luaRune is null) return false;

        luaRune.OnUse(player, rune, isHotkey);

        return false;
    }
}