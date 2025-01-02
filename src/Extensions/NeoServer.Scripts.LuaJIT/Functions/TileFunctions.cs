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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Tile", "", LuaCreateTile);
        RegisterMetaMethod(L, "Tile", "__eq", LuaUserdataCompare<ITile>);

        RegisterMethod(L, "Tile", "getPosition", LuaGetPosition);
        RegisterMethod(L, "Tile", "getGround", LuaGetGround);
        RegisterMethod(L, "Tile", "getThing", LuaTileGetThing);
        RegisterMethod(L, "Tile", "getThingCount", LuaTileGetThingCount);
        RegisterMethod(L, "Tile", "getCreatureCount", LuaTileGetCreatureCount);
        RegisterMethod(L, "Tile", "getTopVisibleThing", LuaTileGetTopVisibleThing);

        RegisterMethod(L, "Tile", "getItems", LuaTileGetItems);
        RegisterMethod(L, "Tile", "getItemCount", LuaTileGetItemCount);

        RegisterMethod(L, "Tile", "hasProperty", LuaTileHasProperty);
        RegisterMethod(L, "Tile", "hasFlag", LuaTileHasFlag);

        RegisterMethod(L, "Tile", "queryAdd", LuaTileQueryAdd);
    }

    public static int LuaCreateTile(LuaState L)
    {
        // Tile(x, y, z)
        // Tile(position)
        ITile tile = null;

        if (Lua.IsTable(L, 2))
        {
            var position = GetPosition(L, 2);
            tile = _gameServer.Map.GetTile(position);
        }
        else
        {
            var z = GetNumber<byte>(L, 4);
            var y = GetNumber<ushort>(L, 3);
            var x = GetNumber<ushort>(L, 2);

            var position = new Location(x, y, z);

            tile = _gameServer.Map.GetTile(position);
        }

        PushUserdata(L, tile);
        SetMetatable(L, -1, "Tile");
        return 1;
    }

    public static int LuaTileGetThing(LuaState L)
    {
        // tile:getThing(index)
        var tile = GetUserdata<ITile>(L, 1);
        var index = GetNumber<int>(L, 2);

        if (tile == null || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(L);
            return 1;
        }

        if (dynamicTile.Creatures.Count >= index + 1)
        {
            var creature = dynamicTile.Creatures[index];
            if (creature != null)
            {
                PushUserdata(L, creature);
                SetCreatureMetatable(L, -1, creature);
                return 1;
            }
        }
        else if (dynamicTile.AllItems.Count() >= index + 1)
        {
            var item = dynamicTile.AllItems[index];

            if (item != null)
            {
                PushUserdata(L, item);
                SetItemMetatable(L, -1, item);
                return 1;
            }
        }

        Lua.PushNil(L);

        return 1;
    }

    public static int LuaTileGetThingCount(LuaState L)
    {
        // tile:getThingCount()
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null)
            Lua.PushNumber(L, tile.ThingsCount);
        else
            Lua.PushNil(L);
        return 1;
    }

    public static int LuaTileGetCreatureCount(LuaState L)
    {
        // tile:getCreatureCount()
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null && tile is IDynamicTile dynamicTile)
            Lua.PushNumber(L, dynamicTile.CreaturesCount);
        else
            Lua.PushNil(L);
        return 1;
    }

    public static int LuaTileGetTopVisibleThing(LuaState L)
    {
        // tile:getTopVisibleThing(creature)
        var creature = GetUserdata<ICreature>(L, 2);
        var tile = GetUserdata<ITile>(L, 1);

        if (tile == null || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(L);
            return 1;
        }

        var visibleCreature = dynamicTile.Creatures.FirstOrDefault(c => c.CreatureId == creature.CreatureId);
        if (visibleCreature != null)
        {
            PushUserdata(L, visibleCreature);
            SetCreatureMetatable(L, -1, visibleCreature);
            return 1;
        }

        var visibleItem = dynamicTile.TopItemOnStack;

        if (visibleItem != null)
        {
            PushUserdata(L, visibleItem);
            SetItemMetatable(L, -1, visibleItem);
            return 1;
        }

        Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetPosition(LuaState L)
    {
        // tile:getPosition()
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null)
            PushPosition(L, tile.Location);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetGround(LuaState L)
    {
        // tile:getGround()
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null && tile.TopItemOnStack != null)
        {
            PushUserdata(L, tile.TopItemOnStack);
            SetItemMetatable(L, -1, tile.TopItemOnStack);
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaTileGetItems(LuaState L)
    {
        // tile:getItems()
        var tile = GetUserdata<ITile>(L, 1);

        if (!tile.HasThings || tile is not IDynamicTile dynamicTile)
        {
            Lua.PushNil(L);
            return 1;
        }

        Lua.CreateTable(L, tile.ThingsCount, 0);

        var index = 0;
        foreach (var item in dynamicTile.AllItems)
        {
            PushUserdata(L, item);
            SetItemMetatable(L, -1, item);
            Lua.RawSetI(L, -2, ++index);
        }

        return 1;
    }

    public static int LuaTileGetItemCount(LuaState L)
    {
        // tile:getItemCount()
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null)
            Lua.PushNumber(L, tile.ThingsCount);
        else
            Lua.PushNil(L);

        return 1;
    }


    public static int LuaTileHasProperty(LuaState L)
    {
        // tile:hasProperty(property, item)
        var tile = GetUserdata<ITile>(L, 1);
        if (!tile)
        {
            Lua.PushNil(L);
            return 1;
        }

        IItem itemToExclude = null;
        if (Lua.GetTop(L) >= 3)
            itemToExclude = GetUserdata<IItem>(L, 3);

        var property = GetNumber<ItemPropertyType>(L, 2);

        if (!itemToExclude)
        {
            PushBoolean(L, tile.HasFlag(property.ToTileFlag()));
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
                PushBoolean(L, itemType.HasFlag(property.ToItemFlag()));
                return 1;
            }
        }

        if (tile is IDynamicTile dynamicTile)
        {
            foreach (var tileItem in dynamicTile.AllItems)
            {
                if (itemToExclude.ServerId == tileItem.ServerId)
                    continue;

                PushBoolean(L, tileItem.Metadata.HasFlag(property.ToItemFlag()));
                return 1;
            }
        }

        PushBoolean(L, false);
        return 1;
    }

    public static int LuaTileHasFlag(LuaState L)
    {
        // tile:hasFlag(flag)
        var tile = GetUserdata<ITile>(L, 1);
        if (tile != null)
        {
            var flag = GetNumber<TileFlags>(L, 2);
            Lua.PushBoolean(L, tile.HasFlag(flag));
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaTileQueryAdd(LuaState L)
    {
        // tile:queryAdd(thing, flags)
        //todo: implements flags

        var tile = GetUserdata<ITile>(L, 1);
        if (!tile)
        {
            Lua.PushNil(L);
            return 1;
        }

        var thing = GetUserdata<IThing>(L, 2);
        if (thing is not null && tile is IDynamicTile dynamicTile)
        {
            var flags = GetNumber<TileFlags>(L, 3, 0);

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

            Lua.PushNumber(L, (byte)returnValue);
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }
}