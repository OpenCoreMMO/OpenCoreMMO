using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Helpers;
using NLua;

namespace NeoServer.Scripts.Lua.Functions;

public static class TileFunctions
{
    public static void AddTileFunctions(this NLua.Lua lua)
    {
        lua.DoString("tile_helper = {}");
        lua["tile_helper.removeTopItem"] = RemoveTopItem;
        lua["tile_helper.addItem"] = AddItem;
        lua["tile_helper.addEnterRule"] = AddEnterRule;
    }

    private static void AddEnterRule(Location location, LuaFunction rule)
    {
        var map = IoC.GetInstance<IMap>();
        var tile = map.GetTile(location);
        if (tile is not IDynamicTile dynamicTile) return;

        dynamicTile.CanEnterFunction = creature => (bool)(rule.Call(creature).FirstOrDefault() ?? false);
    }

    private static bool RemoveTopItem(Location location)
    {
        var map = IoC.GetInstance<IMap>();
        var tile = map.GetTile(location);

        var staticToDynamicTileService = IoC.GetInstance<IStaticToDynamicTileService>();

        tile = tile is IStaticTile staticTile ? staticTile.CreateClone(location) : tile;

        var dynamicTile = staticToDynamicTileService.TransformIntoDynamicTile(tile) as IDynamicTile;

        return dynamicTile?.RemoveTopItem(true).Succeeded ?? false;
    }

    private static bool AddItem(Location location, ushort itemId, byte amount = 1)
    {
        var map = IoC.GetInstance<IMap>();
        var tile = map.GetTile(location);

        var staticToDynamicTileService = IoC.GetInstance<IStaticToDynamicTileService>();

        tile = tile is IStaticTile staticTile ? staticTile.CreateClone(location) : tile;

        var dynamicTile = staticToDynamicTileService.TransformIntoDynamicTile(tile) as IDynamicTile;

        var itemFactory = IoC.GetInstance<IItemFactory>();
        var item = itemFactory.Create(itemId, location, new Dictionary<ItemAttribute, IConvertible>
        {
            [ItemAttribute.Count] = amount
        });

        return dynamicTile?.AddItem(item).Succeeded ?? false;
    }
}