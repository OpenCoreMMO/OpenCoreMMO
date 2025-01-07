using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class NpcFunctions : LuaScriptInterface, INpcFunctions
{
    private static IGameCreatureManager _gameCreatureManager;

    public NpcFunctions(IGameCreatureManager gameCreatureManager) : base(nameof(NpcFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Npc", "Creature", LuaNpcCreate);
        RegisterMetaMethod(luaState, "Npc", "__eq", LuaUserdataCompare<INpc>);
    }

    private static int LuaNpcCreate(LuaState luaState)
    {
        // Npc([id or name or userdata])
        INpc npc = null;
        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<uint>(luaState, 2);
            _gameCreatureManager.TryGetCreature(id, out var creature);

            if (creature != null && creature is IMonster)
            {
                npc = creature as INpc;
            }
            else
            {
                Lua.PushNil(luaState);
                return 1;
            }
        }
        else if (IsUserdata(luaState, 2))
        {
            if (GetUserdataType(luaState, 2) != LuaDataType.Npc)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            npc = GetUserdata<INpc>(luaState, 2);
        }
        else
        {
            npc = null;
        }

        if (npc is not null)
        {
            PushUserdata(luaState, npc);
            SetMetatable(luaState, -1, "Npc");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}