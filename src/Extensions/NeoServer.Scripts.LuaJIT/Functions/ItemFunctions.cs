using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Extensions;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemFunctions : LuaScriptInterface, IItemFunctions
{
    private static IItemTransformService _itemTransformService;
    private static IItemTypeStore _itemTypeStore;
    private static IMap _map;
    private static IItemMovementService _itemMovementService;

    public ItemFunctions(
        IItemTransformService itemTransformService,
        IItemTypeStore itemTypeStore,
        IMap map,
        IItemMovementService itemMovementService) : base(nameof(ItemFunctions))

    {
        _itemTransformService = itemTransformService;
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

        RegisterMethod(luaState, "Item", "getUniqueId", LuaItemGetUniqueId);
        RegisterMethod(luaState, "Item", "getActionId", LuaItemGetActionId);
        RegisterMethod(luaState, "Item", "setActionId", LuaItemSetActionId);

        RegisterMethod(luaState, "Item", "getSubType", LuaItemGetSubType);

        RegisterMethod(luaState, "Item", "getName", LuaItemGetName);
        RegisterMethod(luaState, "Item", "getPluralName", LuaItemGetPluralName);
        RegisterMethod(luaState, "Item", "getArticle", LuaItemGetArticle);

        RegisterMethod(luaState, "Item", "getPosition", LuaItemGetPosition);
        RegisterMethod(luaState, "Item", "getTile", LuaItemGetTile);

        RegisterMethod(luaState, "Item", "hasProperty", LuaItemHasProperty);
        RegisterMethod(luaState, "Item", "hasAttribute", LuaItemHasAttribute);
        RegisterMethod(luaState, "Item", "getAttribute", LuaItemGetAttribute);
        RegisterMethod(luaState, "Item", "setAttribute", LuaItemSetAttribute);
        RegisterMethod(luaState, "Item", "removeAttribute", LuaItemRemoveAttribute);
        RegisterMethod(luaState, "Item", "hasCustomAttribute", LuaItemHasCustomAttribute);
        RegisterMethod(luaState, "Item", "getCustomAttribute", LuaItemGetCustomAttribute);
        RegisterMethod(luaState, "Item", "setCustomAttribute", LuaItemSetCustomAttribute);
        RegisterMethod(luaState, "Item", "removeCustomAttribute", LuaItemRemoveCustomAttribute);

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
            return 1;
        }

        if (item.Owner is not null && item.Owner is IPlayer player)
        {
            var result = player.Inventory.RemoveItem(item.ServerId, (byte)count, true);
            Lua.PushBoolean(luaState, result.Succeeded);
            return 1;
        }

        Lua.PushBoolean(luaState, false);
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

    public static int LuaItemSetActionId(LuaState luaState)
    {
        // item:setActionId(id)
        var item = GetUserdata<IItem>(luaState, 1);
        var actionId = GetNumber<ushort>(luaState, 2);
        if (item != null)
        {
            item.Attributes.SetAttribute(ItemAttribute.ActionId, actionId);
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }

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
        {
            Lua.PushNil(luaState);
        }

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
        // item:hasAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);

        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attribute = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attribute = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attribute = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        Lua.PushBoolean(luaState, item.Attributes.HasAttribute(attribute.ToItemAttribute()));

        return 1;
    }

    public static int LuaItemGetAttribute(LuaState luaState)
    {
        // item:getAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);

        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attribute = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attribute = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attribute = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        if (attribute.IsAttributeInteger())
            Lua.PushNumber(luaState, item.Attributes.GetAttribute<long>(attribute.ToItemAttribute()));
        else if (attribute.IsAttributeString())
            Lua.PushString(luaState, item.Attributes.GetAttribute(attribute.ToItemAttribute()));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemSetAttribute(LuaState luaState)
    {
        // item:setAttribute(key, value)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attributeType = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attributeType = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attributeType = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        var attribute = attributeType.ToItemAttribute();

        //todo: implement start decay?
        // DecayState especial
        //if (attribute == ItemAttribute.DecayState)
        //{
        //    var decayState = GetNumber<ItemDecayStateType>(luaState, 3);
        //    if (decayState == ItemDecayStateType.DECAYING_FALSE || decayState == ItemDecayStateType.DECAYING_STOPPING)
        //        Decay.Instance.Stop(item);
        //    else
        //        Decay.Instance.Start(item);

        //    Lua.PushBoolean(luaState, true);
        //    return 1;
        //}

        //todo: implement start duration?
        //// Duration
        //if (attribute == ItemAttribute.Duration)
        //{
        //    item.Decaying = ItemDecayStateType.DecayingPending;
        //    var duration = GetNumber<uint>(luaState, 3);
        //    item.SetAttribute(ItemAttribute.Duration, duration);
        //    Decay.Instance.Start(item);

        //    Lua.PushBoolean(luaState, true);
        //    return 1;
        //}

        if (attribute == ItemAttribute.Duration)
        {
            ReportError("Attempt to set protected key 'duration timestamp'");
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        if (attributeType.IsAttributeInteger())
        {
            var value = GetNumber<long>(luaState, 3);

            item.Attributes.SetAttribute(attribute, value);
            //todo: check if need this update tile flags
            //item.UpdateTileFlags();
            Lua.PushBoolean(luaState, true);
        }
        else if (attributeType.IsAttributeString())
        {
            var value = GetString(luaState, 3);
            item.Attributes.SetAttribute(attribute, value);
            //todo: check if need this update tile flags
            //item.UpdateTileFlags();
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaItemRemoveAttribute(LuaState luaState)
    {
        // item:removeAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attributeType = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attributeType = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attributeType = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        var attribute = attributeType.ToItemAttribute();

        var canRemove = attribute is not ItemAttribute.UniqueId and not ItemAttribute.Duration;

        if (canRemove)
        {
            item.Attributes.RemoveAttribute(attribute);
        }
        else
        {
            ReportError(attribute == ItemAttribute.UniqueId
                ? "Attempt to erase protected key 'uid'"
                : "Attempt to erase protected key 'duration timestamp'");
        }

        Lua.PushBoolean(luaState, canRemove);
        return 1;
    }

    public static int LuaItemHasCustomAttribute(LuaState luaState)
    {
        // item:hasCustomAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);

        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        string key = Lua.IsNumber(luaState, 2)
            ? GetNumber<long>(luaState, 2).ToString()
            : Lua.IsString(luaState, 2)
                ? GetString(luaState, 2)
                : null;

        Lua.PushBoolean(luaState, key != null && item.Attributes.HasCustomAttribute(key));
        return 1;
    }


    public static int LuaItemGetCustomAttribute(LuaState luaState)
    {
        // item:getCustomAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        string key = Lua.IsNumber(luaState, 2)
            ? GetNumber<long>(luaState, 2).ToString()
            : Lua.IsString(luaState, 2)
                ? GetString(luaState, 2)
                : null;

        if (key == null || !item.Attributes.TryGetCustomAttribute<object>(key, out var value))
        {
            Lua.PushNil(luaState);
            return 1;
        }

        switch (value)
        {
            case bool boolVal:
                Lua.PushBoolean(luaState, boolVal);
                break;
            case double doubleVal:
                Lua.PushNumber(luaState, doubleVal);
                break;
            case long longVal:
                Lua.PushNumber(luaState, longVal);
                break;
            case int intVal:
                Lua.PushNumber(luaState, intVal);
                break;
            case ushort ushortVal:
                Lua.PushNumber(luaState, ushortVal);
                break;
            case byte byteVal:
                Lua.PushNumber(luaState, byteVal);
                break;
            case string strVal:
                Lua.PushString(luaState, strVal);
                break;
            default:
                Lua.PushNil(luaState);
                break;
        }

        return 1;
    }

    public static int LuaItemSetCustomAttribute(LuaState luaState)
    {
        // item:setCustomAttribute(key, value)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        string key = Lua.IsNumber(luaState, 2)
            ? GetNumber<long>(luaState, 2).ToString()
            : Lua.IsString(luaState, 2)
                ? GetString(luaState, 2)
                : null;

        if (key == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        if (Lua.IsNumber(luaState, 3))
        {
            var number = GetNumber<double>(luaState, 3);
            if (Math.Floor(number) == number)
                item.Attributes.SetCustomAttribute(key, Convert.ToInt64(number));
            else
                item.Attributes.SetCustomAttribute(key, number);
        }
        else if (Lua.IsString(luaState, 3))
        {
            item.Attributes.SetCustomAttribute(key, GetString(luaState, 3));
        }
        else if (Lua.IsBoolean(luaState, 3))
        {
            item.Attributes.SetCustomAttribute(key, GetBoolean(luaState, 3));
        }

        var attribute = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attribute = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attribute = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        Lua.PushBoolean(luaState, item.Attributes.HasAttribute(attribute.ToItemAttribute()));

        return 1;
    }

    public static int LuaItemRemoveCustomAttribute(LuaState luaState)
    {
        // item:removeCustomAttribute(key)
        var item = GetUserdata<IItem>(luaState, 1);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        string key = Lua.IsNumber(luaState, 2)
            ? GetNumber<long>(luaState, 2).ToString()
            : Lua.IsString(luaState, 2)
                ? GetString(luaState, 2)
                : null;

        if (key == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        item.Attributes.RemoveCustomAttribute(key);

        Lua.PushBoolean(luaState, true);
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
        dynamicToTile.TryGetStackPositionOfItem(dynamicToTile.TopDownItemOnStack, out var stackPositionTopItem);
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

        var result = _itemTransformService.Transform(item, itemId);

        if (result.Succeeded && result.Value != item)
        {
            env.RemoveItemByUID(uid);
            env.InsertItem(uid, result.Value);

            UpdateLuaUserdata(luaState, 1, result.Value);
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
                it.Attributes.SetAttribute(ItemTypeAttribute.DecayTo, decayTo);
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