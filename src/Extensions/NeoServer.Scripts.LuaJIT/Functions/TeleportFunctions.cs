using LuaNET;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class TeleportFunctions : LuaScriptInterface, ITeleportFunctions
{
    public TeleportFunctions() : base(nameof(TeleportFunctions))
    {
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Teleport", "Item", LuaTeleportCreate);
        RegisterMetaMethod(L, "Teleport", "__eq", LuaUserdataCompare<ITeleport>);
    }

    private static int LuaTeleportCreate(LuaState L)
    {
        // Teleport(uid)
        var id = GetNumber<uint>(L, 2);

        var item = GetScriptEnv().GetItemByUID(id);
        if (item != null)
        {
            PushUserdata(L, item);
            SetMetatable(L, -1, "Teleport");
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }
}
