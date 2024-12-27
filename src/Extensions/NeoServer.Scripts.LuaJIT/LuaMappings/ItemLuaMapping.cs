using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class ItemLuaMapping : LuaScriptInterface, IItemLuaMapping
{
    private static IItemTransformService _itemTransformService;
    private static IItemTypeStore _itemTypeStore;
    private static IMap _map;
    private static IItemMovementService _itemMovementService;

    public ItemLuaMapping(
        IItemTransformService itemTransformService,
        IItemTypeStore itemTypeStore,
        IMap map,
        IItemMovementService itemMovementService) : base(nameof(ItemLuaMapping))
        
    {
        _itemTransformService = itemTransformService;
        _itemTypeStore = itemTypeStore;
        _map = map;
        _itemMovementService = itemMovementService;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Item", "", LuaCreateItem);
        RegisterMetaMethod(L, "Item", "__eq", LuaUserdataCompare<IItem>);

        RegisterMethod(L, "Item", "getId", LuaGetItemId);

        RegisterMethod(L, "Item", "getActionId", LuaGetActionId);
        RegisterMethod(L, "Item", "getUniqueId", LuaGetUniqueId);

        RegisterMethod(L, "Item", "getSubType", LuaGetSubType);

        RegisterMethod(L, "Item", "hasProperty", LuaItemHasProperty);

        RegisterMethod(L, "Item", "moveTo", LuaItemMoveTo);
        RegisterMethod(L, "Item", "transform", LuaItemTransform);
        RegisterMethod(L, "Item", "decay", LuaItemDecay);
    }

    public static int LuaCreateItem(LuaState L)
    {
        // Item(uid)
        var id = GetNumber<uint>(L, 2);

        var item = GetScriptEnv().GetItemByUID(id);
        if (item != null)
        {
            PushUserdata(L, item);
            SetMetatable(L, -1, "Item");
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaGetItemId(LuaState L)
    {
        // item:getId()
        var item = GetUserdata<IItem>(L, 1);
        if (item != null)
            Lua.PushNumber(L, item.ServerId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetActionId(LuaState L)
    {
        // item:getActionId()
        var item = GetUserdata<IItem>(L, 1);
        if (item != null)
            Lua.PushNumber(L, item.ActionId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetUniqueId(LuaState L)
    {
        // item:getUniqueId()
        var item = GetUserdata<IItem>(L, 1);
        if (item != null)
            Lua.PushNumber(L, item.UniqueId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetSubType(LuaState L)
    {
        // item:getSubType()
        var item = GetUserdata<IItem>(L, 1);
        if (item != null)
            Lua.PushNumber(L, item.GetSubType());
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemHasProperty(LuaState L)
    {
        // item:hasProperty()
        var item = GetUserdata<IItem>(L, 1);
        if (item != null)
        {
            var property = GetNumber<ItemFlag>(L, 2);
            Lua.PushBoolean(L, item.Metadata.HasFlag(property));
        }
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemMoveTo(LuaState L)
    {
        // item:moveTo(position or cylinder[, flags])
        //todo: implements flags
        var item = GetUserdata<IItem>(L, 1);
        if (item == null)
        {
            Lua.PushNil(L);
            return 1;
        }

        //const auto &item = *itemPtr;
        //if (!item || item->isRemoved())
        //{
        //    lua_pushnil(L);
        //    return 1;
        //}

        IContainer toContainer = null;
        IPlayer toPlayer = null;
        ITile toTile = null;

        ushort itemId = 0;
        if (Lua.IsUserData(L, 2))
        {
            LuaDataType type = GetUserdataType(L, 2);
            switch (type)
            {
                case LuaDataType.Container:
                    toContainer = GetUserdata<IContainer>(L, 2);
                    break;
                case LuaDataType.Player:
                    toPlayer = GetUserdata<IPlayer>(L, 2);
                    break;
                case LuaDataType.Tile:
                    toTile = GetUserdata<ITile>(L, 2);
                    break;
                default:
                    break;
            }
        }
        else
        {
            toTile = _map.GetTile(GetPosition(L, 2));
        }

        if (toContainer == null &&
            toPlayer == null &&
            toTile == null)
        {
            Lua.PushNil(L);
            return 1;
        }

        if (item.Parent != null &&
            (item.Parent == toContainer ||
            item.Parent == toPlayer ||
            item.Parent == toTile))
        {
            Lua.PushBoolean(L, true);
            return 1;
        }

        var fromTile = _map.GetTile(item.Location);

        if (toTile is not IDynamicTile dynamicToTile ||
            fromTile is not IDynamicTile dynamicFromTile)
        {
            Lua.PushBoolean(L, true);
            return 1;
        }

        dynamicFromTile.TryGetStackPositionOfItem(item, out var stackPosition);
        dynamicToTile.TryGetStackPositionOfItem(dynamicToTile.TopItemOnStack, out var stackPositionTopItem);
        var result = _itemMovementService.Move(item, dynamicFromTile, dynamicToTile, item.Amount, stackPosition, (byte)(dynamicToTile.ItemsCount + 1));

        if (result.Succeeded)
            Lua.PushBoolean(L, true);
        else
            Lua.PushBoolean(L, false);

        return 1;
    }

    public static int LuaItemTransform(LuaState L)
    {
        // item:transform(itemId[, count/subType = -1])
        var item = GetUserdata<IItem>(L, 1);
        if (item == null)
        {
            Lua.PushNil(L);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(L, 2))
        {
            itemId = GetNumber<ushort>(L, 2);
        }
        else
        {
            var itemName = GetString(L, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null)
            {
                Lua.PushNil(L);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var subType = GetNumber<int>(L, 3, -1);

        if (item.ServerId == itemId && (subType == -1 || subType == item.GetSubType()))
        {
            Lua.PushBoolean(L, true);
            return 1;
        }

        var it = _itemTypeStore.Get(itemId);
        if (it.IsStackable())
        {
            subType = int.Min(subType, it.Count);
        }

        var env = GetScriptEnv();
        var uid = env.AddThing(item);

        var result = _itemTransformService.Transform(item, itemId);
        var newItem = result.Value;

        if (result.Succeeded)
        {
            env.RemoveItemByUID(uid);
        }

        if (newItem != null && newItem != item)
        {
            env.InsertItem(uid, newItem);
        }

        item = newItem;
        Lua.PushBoolean(L, true);
        return 1;
    }

    public static int LuaItemDecay(LuaState L)
    {
        // item:decay(decayId)
        var item = GetUserdata<IItem>(L, 1);
        if (item != null && item.Decay != null)
        {
            if (Lua.IsNumber(L, 2))
            {
                var it = _itemTypeStore.Get(item.ServerId);
                var decayTo = GetNumber<int>(L, 2);
                it.Attributes.SetAttribute(ItemAttribute.DecayTo, decayTo);
                item.UpdateMetadata(it);
            }

            item.Decay.StartDecay();
            Lua.PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }
}
