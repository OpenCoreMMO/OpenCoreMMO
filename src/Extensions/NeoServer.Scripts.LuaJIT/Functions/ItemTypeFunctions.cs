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

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "ItemType", "", LuaCreateItemType);
        RegisterMetaMethod(luaState, "ItemType", "__eq", LuaUserdataCompare<IItemType>);

        RegisterMethod(luaState, "ItemType", "isMovable", LuaItemTypeIsMoveable);
        RegisterMethod(luaState, "ItemType", "isStackable", LuaItemTypeIsStackable);
        RegisterMethod(luaState, "ItemType", "isFluidContainer", LuaItemTypeIsFluidContainer);
        RegisterMethod(luaState, "ItemType", "isKey", LuaItemTypeIsKey);

        RegisterMethod(luaState, "ItemType", "getType", LuaItemTypeGetType);
        RegisterMethod(luaState, "ItemType", "getId", LuaItemTypeGetId);
        RegisterMethod(luaState, "ItemType", "getName", LuaItemTypeGetName);

        RegisterMethod(luaState, "ItemType", "getWeight", LuaItemTypeGetWeight);

        RegisterMethod(luaState, "ItemType", "getDestroyId", LuaItemDestroyId);
    }

    public static int LuaCreateItemType(LuaState luaState)
    {
        // ItemType(id or name)
        ushort id = 0;
        IItemType itemType = null;
        if (Lua.IsNumber(luaState, 2))
        {
            id = GetNumber<ushort>(luaState, 2);
            itemType = _itemTypeStore.Get(id);
        }
        else
        {
            var name = GetString(luaState, 2);
            itemType = _itemTypeStore.GetByName(name);
        }

        PushUserdata(luaState, itemType);
        SetMetatable(luaState, -1, "ItemType");

        return 1;
    }

    public static int LuaItemTypeIsMoveable(LuaState luaState)
    {
        // itemType:isMovable()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsMovable());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsStackable(LuaState luaState)
    {
        // itemType:isStackable()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsStackable());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsFluidContainer(LuaState luaState)
    {
        // itemType:isFluidContainer()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsFluidContainer());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsKey(LuaState luaState)
    {
        // itemType:isKey()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsKey());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetType(LuaState luaState)
    {
        // itemType:getType()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.ServerId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetId(LuaState luaState)
    {
        // itemType:getId()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.ServerId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetName(LuaState luaState)
    {
        // itemType:getName()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushString(luaState, itemType.Name);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetWeight(LuaState luaState)
    {
        // itemType:getWeight(count = 1)
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
        {
            var count = GetNumber<ushort>(luaState, 2, 1);
            var weight = itemType.Weight * float.Max(1, count);
            Lua.PushNumber(luaState, weight);
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemDestroyId(LuaState luaState)
    {
        // itemType:getDestroyId()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.DestroyTo);
        else
            Lua.PushNil(luaState);

        return 1;
    }
}
