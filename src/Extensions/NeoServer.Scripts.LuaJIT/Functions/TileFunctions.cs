using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class TileFunctions : LuaScriptInterface, ITileFunctions
{
    private static IGameServer _gameServer;
    private static IItemTypeStore _itemTypeStore; 
    private static IItemClientServerIdMapStore _itemClientServerIdMapStore;

    public TileFunctions(IGameServer gameServer) : base(nameof(TileFunctions))
    {
        _gameServer = gameServer;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Tile", "", LuaCreateTile);
        RegisterMetaMethod(luaState, "Tile", "__eq", LuaUserdataCompare<ITile>);

        RegisterMethod(luaState, "Tile", "getPosition", LuaGetPosition);
        RegisterMethod(luaState, "Tile", "getGround", LuaGetGround);
        RegisterMethod(luaState, "Tile", "getThing", LuaTileGetThing);
        RegisterMethod(luaState, "Tile", "getThingCount", LuaTileGetThingCount);
        RegisterMethod(luaState, "Tile", "getCreatureCount", LuaTileGetCreatureCount);
        RegisterMethod(luaState, "Tile", "getTopVisibleThing", LuaTileGetTopVisibleThing);

        RegisterMethod(luaState, "Tile", "getItems", LuaTileGetItems);
        RegisterMethod(luaState, "Tile", "getItemCount", LuaTileGetItemCount);

        RegisterMethod(luaState, "Tile", "hasProperty", LuaTileHasProperty);
        RegisterMethod(luaState, "Tile", "hasFlag", LuaTileHasFlag);

        RegisterMethod(luaState, "Tile", "queryAdd", LuaTileQueryAdd);
    }

    public static int LuaCreateTile(LuaState luaState)
    {
        // Tile(x, y, z)
        // Tile(position)
        ITile tile = null;

        if (Lua.IsTable(luaState, 2))
        {
            var position = GetPosition(luaState, 2);
            tile = _gameServer.Map.GetTile(position);
        }
        else
        {
            var z = GetNumber<byte>(luaState, 4);
            var y = GetNumber<ushort>(luaState, 3);
            var x = GetNumber<ushort>(luaState, 2);

            var position = new Location(x, y, z);

            tile = _gameServer.Map.GetTile(position);
        }

        PushUserdata(luaState, tile);
        SetMetatable(luaState, -1, "Tile");
        return 1;
    }

    public static int LuaTileGetThing(LuaState luaState)
    {
        // tile:getThing(index)
        var tile = GetUserdata<ITile>(luaState, 1);
        var index = GetNumber<int>(luaState, 2);

        if (tile == null || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        if (dynamicTile.Creatures.Count >= index + 1)
        {
            var creature = dynamicTile.Creatures[index];
            if (creature != null)
            {
                PushUserdata(luaState, creature);
                SetCreatureMetatable(luaState, -1, creature);
                return 1;
            }
        }
        else if (dynamicTile.AllItems.Count() >= index + 1)
        {
            var item = dynamicTile.AllItems[index];

            if (item != null)
            {
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
                return 1;
            }
        }

        Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaTileGetThingCount(LuaState luaState)
    {
        // tile:getThingCount()
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null)
            Lua.PushNumber(luaState, tile.ThingsCount);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    public static int LuaTileGetCreatureCount(LuaState luaState)
    {
        // tile:getCreatureCount()
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null && tile is IDynamicTile dynamicTile)
            Lua.PushNumber(luaState, dynamicTile.CreaturesCount);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    public static int LuaTileGetTopVisibleThing(LuaState luaState)
    {
        // tile:getTopVisibleThing(creature)
        var creature = GetUserdata<ICreature>(luaState, 2);
        var tile = GetUserdata<ITile>(luaState, 1);

        if (tile == null || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var visibleCreature = dynamicTile.Creatures.FirstOrDefault(c => c.CreatureId == creature.CreatureId);
        if (visibleCreature != null)
        {
            PushUserdata(luaState, visibleCreature);
            SetCreatureMetatable(luaState, -1, visibleCreature);
            return 1;
        }

        var visibleItem = dynamicTile.TopItemOnStack;

        if (visibleItem != null)
        {
            PushUserdata(luaState, visibleItem);
            SetItemMetatable(luaState, -1, visibleItem);
            return 1;
        }

        Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGetPosition(LuaState luaState)
    {
        // tile:getPosition()
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null)
            PushPosition(luaState, tile.Location);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGetGround(LuaState luaState)
    {
        // tile:getGround()
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null && tile.TopItemOnStack != null)
        {
            PushUserdata(luaState, tile.TopItemOnStack);
            SetItemMetatable(luaState, -1, tile.TopItemOnStack);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaTileGetItems(LuaState luaState)
    {
        // tile:getItems()
        var tile = GetUserdata<ITile>(luaState, 1);

        if (!tile.HasThings || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.CreateTable(luaState, tile.ThingsCount, 0);

        var index = 0;
        foreach (var item in dynamicTile.AllItems)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
            Lua.RawSetI(luaState, -2, ++index);
        }

        return 1;
    }

    public static int LuaTileGetItemCount(LuaState luaState)
    {
        // tile:getItemCount()
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null)
            Lua.PushNumber(luaState, tile.ThingsCount);
        else
            Lua.PushNil(luaState);

        return 1;
    }


    public static int LuaTileHasProperty(LuaState luaState)
    {
        // tile:hasProperty(property, item)
        var tile = GetUserdata<ITile>(luaState, 1);
        if (!tile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        IItem itemToExclude = null;
        if (Lua.GetTop(luaState) >= 3)
            itemToExclude = GetUserdata<IItem>(luaState, 3);

        var property = GetNumber<ItemPropertyType>(luaState, 2);

        if (!itemToExclude)
        {
            PushBoolean(luaState, tile.HasFlag(property.ToTileFlag()));
            return 1;
        }

        if (tile is IStaticTile staticTile)
        {
            foreach (var itemClientId in staticTile.AllClientIdItems)
            {
                _itemClientServerIdMapStore.TryGetValue(itemClientId, out var itemServerId);

                if (itemToExclude.ServerId == itemServerId)
                    continue;

                _itemTypeStore.TryGetValue(itemServerId, out var itemType);
                PushBoolean(luaState, itemType.HasFlag(property.ToItemFlag()));
                return 1;
            }
        }

        if (tile is IDynamicTile dynamicTile)
        {
            foreach (var tileItem in dynamicTile.AllItems)
            {
                if (itemToExclude.ServerId == tileItem.ServerId)
                    continue;

                PushBoolean(luaState, tileItem.Metadata.HasFlag(property.ToItemFlag()));
                return 1;
            }
        }

        PushBoolean(luaState, false);
        return 1;
    }

    public static int LuaTileHasFlag(LuaState luaState)
    {
        // tile:hasFlag(flag)
        var tile = GetUserdata<ITile>(luaState, 1);
        if (tile != null)
        {
            var flag = GetNumber<TileFlags>(luaState, 2);
            Lua.PushBoolean(luaState, tile.HasFlag(flag));
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaTileQueryAdd(LuaState luaState)
    {
        // tile:queryAdd(thing, flags)
        //todo: implements flags

        var tile = GetUserdata<ITile>(luaState, 1);
        if (!tile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var thing = GetUserdata<IThing>(luaState, 2);
        if (thing is not null && tile is IDynamicTile dynamicTile)
        {
            var flags = GetNumber<TileFlags>(luaState, 3, 0);

            var returnValue = ReturnValueType.RETURNVALUE_NOTPOSSIBLE;

            if (thing is ICreature creature)
            {
                if (dynamicTile.CanEnter(creature))
                    returnValue = ReturnValueType.RETURNVALUE_NOERROR;
            }
            else if (thing is IItem item)
            {
                if (dynamicTile.CanAddItem(item, item.Amount).Succeeded)
                    returnValue = ReturnValueType.RETURNVALUE_NOERROR;
            }

            Lua.PushNumber(luaState, (byte)returnValue);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}