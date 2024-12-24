using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Item;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemFunctions : LuaScriptInterface, IItemFunctions
{
    private static IItemTransformService _itemTransformService;
    private static IItemTypeStore _itemTypeStore;

    public ItemFunctions(
        IItemTransformService itemTransformService,
        IItemTypeStore itemTypeStore) : base(nameof(ItemFunctions))
    {
        _itemTransformService = itemTransformService;
        _itemTypeStore = itemTypeStore;
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
