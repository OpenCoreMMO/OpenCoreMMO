using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;

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
        RegisterMethod(L, "ItemType", "getType", LuaGetItemTypeType);
        RegisterMethod(L, "ItemType", "getId", LuaGetItemTypeId);
        RegisterMethod(L, "ItemType", "getName", LuaGetItemTypeName);
        RegisterMethod(L, "ItemType", "isMovable", LuaGetItemTypeIsMoveable);
    }

    public static int LuaCreateItemType(LuaState L)
    {
        // ItemType(id or name)
        ushort id = 0;
        IItemType itemType = null;
        if(Lua.IsNumber(L, 2))
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

    public static int LuaGetItemTypeType(LuaState L)
    {
        // itemType:getType()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushNumber(L, itemType.ServerId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetItemTypeId(LuaState L)
    {
        // itemType:getId()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushNumber(L, itemType.ServerId);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetItemTypeName(LuaState L)
    {
        // item:getName()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushString(L, itemType.Name);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGetItemTypeIsMoveable(LuaState L)
    {
        // item:isMovable()
        var itemType = GetUserdata<IItemType>(L, 1);
        if (itemType != null)
            Lua.PushBoolean(L, itemType.HasFlag(ItemFlag.MovementEvent));
        else
            Lua.PushNil(L);

        return 1;
    }
}
