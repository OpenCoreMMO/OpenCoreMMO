using LuaNET;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ContainerFunctions : LuaScriptInterface, IContainerFunctions
{
    public ContainerFunctions() : base(nameof(ContainerFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Container", "Item", LuaContainerCreate);
        RegisterMetaMethod(luaState, "Container", "__eq", LuaUserdataCompare<IContainer>);

        RegisterMethod(luaState, "Container", "getSize", LuaContainerGetSize);

        RegisterMethod(luaState, "Container", "getItem", LuaContainerGetItem);

    }

    private static int LuaContainerCreate(LuaState luaState)
    {
        // Container(uid)
        var id = GetNumber<uint>(luaState, 2);

        var container = GetScriptEnv().GetContainerByUID(id);
        if (container != null)
        {
            PushUserdata(luaState, container);
            SetMetatable(luaState, -1, "Container");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaContainerGetSize(LuaState luaState)
    {
        // container:getSize()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
            Lua.PushNumber(luaState, container.Items.Count);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaContainerGetItem(LuaState luaState)
    {
        // container:getItem(index)
        var container = GetUserdata<IContainer>(luaState, 1);

        if (!container)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var index = GetNumber<int>(luaState, 2);
        var item = container.Items.ElementAtOrDefault(index);

        if (item != null)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

}
