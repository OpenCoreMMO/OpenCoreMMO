using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemFunctions : LuaScriptInterface, IItemFunctions
{
    private static IItemService _itemService;
    private static IItemTypeStore _itemTypeStore;
    private static IMap _map;
    private static IItemMovementService _itemMovementService;

    public ItemFunctions(
        IItemService itemService,
        IItemTypeStore itemTypeStore,
        IMap map,
        IItemMovementService itemMovementService) : base(nameof(ItemFunctions))

    {
        _itemService = itemService;
        _itemTypeStore = itemTypeStore;
        _map = map;
        _itemMovementService = itemMovementService;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Item", "", LuaCreateItem);
        RegisterMetaMethod(luaState, "Item", "__eq", LuaUserdataCompare<IItem>);

        RegisterMethod(luaState, "Item", "isItem", LuaItemIsItem);

        RegisterMethod(luaState, "Item", "getId", LuaItemGetId);

        RegisterMethod(luaState, "Item", "remove", LuaItemRemove);

        RegisterMethod(luaState, "Item", "getActionId", LuaItemGetActionId);
        RegisterMethod(luaState, "Item", "getUniqueId", LuaItemGetUniqueId);

        RegisterMethod(luaState, "Item", "getSubType", LuaItemGetSubType);

        RegisterMethod(luaState, "Item", "getName", LuaItemGetName);
        RegisterMethod(luaState, "Item", "getPluralName", LuaItemGetPluralName);
        RegisterMethod(luaState, "Item", "getArticle", LuaItemGetArticle);

        RegisterMethod(luaState, "Item", "getPosition", LuaItemGetPosition);
        RegisterMethod(luaState, "Item", "getTile", LuaItemGetTile);

        RegisterMethod(luaState, "Item", "hasProperty", LuaItemHasProperty);
        RegisterMethod(luaState, "Item", "hasAttribute", LuaItemHasAttribute);

        RegisterMethod(luaState, "Item", "moveTo", LuaItemMoveTo);
        RegisterMethod(luaState, "Item", "transform", LuaItemTransform);
        RegisterMethod(luaState, "Item", "decay", LuaItemDecay);
    }

    public static int LuaCreateItem(LuaState luaState)
    {
        // Item(uid)
        var id = GetNumber<uint>(luaState, 2);

        var item = GetScriptEnv().GetItemByUID(id);
        if (item != null)
        {
            PushUserdata(luaState, item);
            SetMetatable(luaState, -1, "Item");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaItemIsItem(LuaState luaState)
    {
        // item:isItem()
        var item = GetUserdata<IItem>(luaState, 1);
        Lua.PushBoolean(luaState, item is not null);

        return 1;
    }

    public static int LuaItemGetId(LuaState luaState)
    {
        // item:getId()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushNumber(luaState, item.ServerId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemRemove(LuaState luaState)
    {
        // item:remove(count = -1)
        var item = GetUserdata<IItem>(luaState, 1);
        var count = GetNumber(luaState, 2, -1);

        if (item is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        if (item is ICumulative cumulative && count > 0)
        {
            cumulative.Reduce((byte)count);
            return 1;
        }

        if (_map[item.Location] is IDynamicTile dynamictile)
        {
            var result = dynamictile.RemoveItem(item, (byte)count, 0, out var removedItem);
            Lua.PushBoolean(luaState, result.Succeeded);
        }

        return 1;
    }

    public static int LuaItemGetActionId(LuaState luaState)
    {
        // item:getActionId()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushNumber(luaState, item.ActionId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetUniqueId(LuaState luaState)
    {
        // item:getUniqueId()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushNumber(luaState, item.UniqueId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetSubType(LuaState luaState)
    {
        // item:getSubType()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushNumber(luaState, item.GetSubType());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetName(LuaState luaState)
    {
        // item:getName()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushString(luaState, item.Name);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetPluralName(LuaState luaState)
    {
        // item:getPluralName()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushString(luaState, item.Plural);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetArticle(LuaState luaState)
    {
        // item:getArticle()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            Lua.PushString(luaState, item.Article);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetPosition(LuaState luaState)
    {
        // item:getPosition()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
            PushPosition(luaState, item.Location);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemGetTile(LuaState luaState)
    {
        // item:getTile()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
        {
            var tile = _map[item.Location];
            PushUserdata(luaState, tile);
            SetMetatable(luaState, -1, "Tile");
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemHasProperty(LuaState luaState)
    {
        // item:hasProperty()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
        {
            var property = GetNumber<ItemFlag>(luaState, 2);
            Lua.PushBoolean(luaState, item.Metadata.HasFlag(property));
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaItemHasAttribute(LuaState luaState)
    {
        // item:hasAttribute()
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
        {
            var property = GetNumber<ItemAttributeType>(luaState, 2);
            Lua.PushBoolean(luaState, item.Metadata.Attributes.HasAttribute(property.ToItemAttribute()));
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemMoveTo(LuaState luaState)
    {
        // item:moveTo(position or cylinder, flags)
        //todo: implements flags
        var item = GetUserdata<IItem>(luaState, 1);
        if (!item)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        //todo: implements isRemoved
        //const auto &item = *itemPtr;
        //if (!item || item->isRemoved())
        //{
        //    lua_pushnil(luaState);
        //    return 1;
        //}

        IContainer toContainer = null;
        IPlayer toPlayer = null;
        ITile toTile = null;

        ushort itemId = 0;
        if (Lua.IsUserData(luaState, 2))
        {
            var type = GetUserdataType(luaState, 2);
            switch (type)
            {
                case LuaDataType.Container:
                    toContainer = GetUserdata<IContainer>(luaState, 2);
                    break;
                case LuaDataType.Player:
                    toPlayer = GetUserdata<IPlayer>(luaState, 2);
                    break;
                case LuaDataType.Tile:
                    toTile = GetUserdata<ITile>(luaState, 2);
                    break;
            }
        }
        else
        {
            toTile = _map.GetTile(GetPosition(luaState, 2));
        }

        if (!toContainer &&
            !toPlayer &&
            !toTile)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        if (item.Parent != null &&
            (item.Parent == toContainer ||
             item.Parent == toPlayer ||
             item.Parent == toTile))
        {
            Lua.PushBoolean(luaState, true);
            return 1;
        }

        var fromTile = _map.GetTile(item.Location);

        if (toTile is not IDynamicTile dynamicToTile ||
            fromTile is not IDynamicTile dynamicFromTile)
        {
            Lua.PushBoolean(luaState, true);
            return 1;
        }

        dynamicFromTile.TryGetStackPositionOfItem(item, out var stackPosition);
        dynamicToTile.TryGetStackPositionOfItem(dynamicToTile.TopItemOnStack, out var stackPositionTopItem);
        var result = _itemMovementService.Move(item, dynamicFromTile, dynamicToTile, item.Amount, stackPosition,
            (byte)(dynamicToTile.ItemsCount + 1));

        if (result.Succeeded)
            Lua.PushBoolean(luaState, true);
        else
            Lua.PushBoolean(luaState, false);

        return 1;
    }

    public static int LuaItemTransform(LuaState luaState)
    {
        // item:transform(itemId, count/subType = -1)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(luaState, 2))
        {
            itemId = GetNumber<ushort>(luaState, 2);
        }
        else
        {
            var itemName = GetString(luaState, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var subType = GetNumber(luaState, 3, -1);

        if (item.ServerId == itemId && (subType == -1 || subType == item.GetSubType()))
        {
            Lua.PushBoolean(luaState, true);
            return 1;
        }

        var it = _itemTypeStore.Get(itemId);
        if (it.IsStackable()) subType = int.Min(subType, it.Count);

        var env = GetScriptEnv();
        var uid = env.AddThing(item);

        var newItem = _itemService.Transform(item.Location, item.ServerId, itemId);

        if (newItem != null && newItem != item)
        {
            env.RemoveItemByUID(uid);
            env.InsertItem(uid, newItem);

            UpdateLuaUserdata(luaState, 1, newItem);
        }

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    public static int LuaItemDecay(LuaState luaState)
    {
        // item:decay(decayId)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item != null)
        {
            if (Lua.IsNumber(luaState, 2))
            {
                var it = _itemTypeStore.Get(item.ServerId);
                var decayTo = GetNumber<int>(luaState, 2);
                it.Attributes.SetAttribute(ItemAttribute.DecayTo, decayTo);
                item.UpdateMetadata(it);
            }

            item.Decay?.StartDecay();
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}