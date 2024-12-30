using LuaNET;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ContainerFunctions : LuaScriptInterface, IContainerFunctions
{
    public ContainerFunctions() : base(nameof(ContainerFunctions))
    {
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Container", "Item", LuaContainerCreate);
        RegisterMetaMethod(L, "Container", "__eq", LuaUserdataCompare<IContainer>);

        RegisterMethod(L, "Container", "getSize", LuaContainerGetSize);

        RegisterMethod(L, "Container", "getItem", LuaContainerGetItem);

    }

    private static int LuaContainerCreate(LuaState L)
    {
        // Container(uid)
        var id = GetNumber<uint>(L, 2);

        var container = GetScriptEnv().GetContainerByUID(id);
        if (container != null)
        {
            PushUserdata(L, container);
            SetMetatable(L, -1, "Container");
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaContainerGetSize(LuaState L)
    {
        // container:getSize()
        var container = GetUserdata<IContainer>(L, 1);
        if (container != null)
            Lua.PushNumber(L, container.Items.Count);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaContainerGetItem(LuaState L)
    {
        // container:getItem(index)
        var container = GetUserdata<IContainer>(L, 1);

        if (!container)
        {
            Lua.PushNil(L);
            return 1;
        }

        var index = GetNumber<int>(L, 2);
        var item = container.Items.ElementAtOrDefault(index);

        if (item != null)
        {
            PushUserdata(L, item);
            SetItemMetatable(L, -1, item);
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

}
