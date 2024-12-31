using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemTypeFunctions : LuaScriptInterface, IItemTypeFunctions
{
    private static IItemTypeStore _itemTypeStore;

    public ItemTypeFunctions(IItemTypeStore itemTypeStore) : base(nameof(ItemTypeFunctions))
    {
        _itemTypeStore = itemTypeStore;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "ItemType", "", LuaCreateItemType);
        RegisterMetaMethod(L, "ItemType", "__eq", LuaUserdataCompare<IItemType>);

        RegisterMethod(L, "ItemType", "isMovable", LuaItemTypeIsMoveable);
        RegisterMethod(L, "ItemType", "isStackable", LuaItemTypeIsStackable);
        RegisterMethod(L, "ItemType", "isFluidContainer", LuaItemTypeIsFluidContainer);
        RegisterMethod(L, "ItemType", "isKey", LuaItemTypeIsKey);

        RegisterMethod(L, "ItemType", "getType", LuaItemTypeGetType);
        RegisterMethod(L, "ItemType", "getId", LuaItemTypeGetId);
        RegisterMethod(L, "ItemType", "getName", LuaItemTypeGetName);

        RegisterMethod(L, "ItemType", "getWeight", LuaItemTypeGetWeight);

        RegisterMethod(L, "ItemType", "getDestroyId", LuaItemDestroyId);
    }

    public static int LuaCreateItemType(LuaState L)
    {
        // ItemType(id or name)
        ushort id = 0;
        IItemType itemType = null;
        if (Lua.IsNumber(L, 2))
        {
            id = GetNumber<ushort>(L, 2);
            itemType = _itemTypeStore.Get(id);
        }
        else
        {
            var name = GetString(L, 2);
            itemType = _itemTypeStore.GetByName(name);
        }

        PushUserdata(L, itemType);
        SetMetatable(L, -1, "ItemType");

        return 1;
    }

    public static int LuaItemTypeIsMoveable(LuaState L)
    {
        // itemType:isMovable()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushBoolean(L, itemType.IsMovable());
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeIsStackable(LuaState L)
    {
        // itemType:isStackable()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushBoolean(L, itemType.IsStackable());
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeIsFluidContainer(LuaState L)
    {
        // itemType:isFluidContainer()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushBoolean(L, itemType.IsFluidContainer());
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeIsKey(LuaState L)
    {
        // itemType:isKey()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushBoolean(L, itemType.IsKey());
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeGetType(LuaState L)
    {
        // itemType:getType()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushNumber(L, itemType.ServerId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeGetId(LuaState L)
    {
        // itemType:getId()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushNumber(L, itemType.ServerId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeGetName(LuaState L)
    {
        // itemType:getName()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushString(L, itemType.Name);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemTypeGetWeight(LuaState L)
    {
        // itemType:getWeight(count = 1)
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
        {
            var count = GetNumber<ushort>(L, 2, 1);
            var weight = itemType.Weight * float.Max(1, count);
            Lua.PushNumber(L, weight);
        }
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaItemDestroyId(LuaState L)
    {
        // itemType:getDestroyId()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushNumber(L, itemType.DestroyTo);
        else
            Lua.PushNil(L);

        return 1;
    }
}
