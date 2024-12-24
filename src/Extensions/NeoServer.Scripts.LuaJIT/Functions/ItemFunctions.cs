using LuaNET;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemFunctions : LuaScriptInterface, IItemFunctions
{
    public ItemFunctions(
        ILuaEnvironment luaEnvironment, 
        ILogger logger) : base(nameof(ItemFunctions))
    {
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
    }

    //todo: implements this
    public static int LuaCreateItem(LuaState L)
    {
        // Item(uid)
        var id = GetNumber<int>(L, 2);

        //const auto &item = GetScriptEnv().GetItemByUID(id);
        //if (item)
        //{
        //    Lua::pushUserdata<Item>(L, item);
        //    Lua::setItemMetatable(L, -1, item);
        //}
        //else
        //{
        //    lua_pushnil(L);
        //}
        //return 1;


        // Item(uid)
        IItem item = null;
        PushUserdata(L, item);
        SetMetatable(L, -1, "Item");

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
}
