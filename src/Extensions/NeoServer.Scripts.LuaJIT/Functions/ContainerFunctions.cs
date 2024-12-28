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
}
