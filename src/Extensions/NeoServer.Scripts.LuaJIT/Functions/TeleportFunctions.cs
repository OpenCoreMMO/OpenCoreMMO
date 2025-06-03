using LuaNET;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class TeleportFunctions : LuaScriptInterface, ITeleportFunctions
{
    public TeleportFunctions() : base(nameof(TeleportFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Teleport", "Item", LuaTeleportCreate);
        RegisterMetaMethod(luaState, "Teleport", "__eq", LuaUserdataCompare<ITeleport>);
    }

    private static int LuaTeleportCreate(LuaState luaState)
    {
        // Teleport(uid)
        var id = GetNumber<uint>(luaState, 2);

        var item = GetScriptEnv().GetItemByUID(id);
        if (item != null)
        {
            PushUserdata(luaState, item);
            SetMetatable(luaState, -1, "Teleport");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}