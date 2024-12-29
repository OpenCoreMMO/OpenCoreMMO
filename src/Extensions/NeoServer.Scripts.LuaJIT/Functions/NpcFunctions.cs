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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Npc", "Creature", LuaNpcCreate);
        RegisterMetaMethod(L, "Npc", "__eq", LuaUserdataCompare<INpc>);
    }

    private static int LuaNpcCreate(LuaState L)
    {
        // Npc([id or name or userdata])
        INpc npc = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<uint>(L, 2);
            _gameCreatureManager.TryGetCreature(id, out var creature);

            if (creature != null && creature is IMonster)
            {
                npc = creature as INpc;
            }
            else
            {
                Lua.PushNil(L);
                return 1;
            }
        }
        else if (IsUserdata(L, 2))
        {
            if (GetUserdataType(L, 2) != LuaDataType.Npc)
            {
                Lua.PushNil(L);
                return 1;
            }

            npc = GetUserdata<INpc>(L, 2);
        }
        else
        {
            npc = null;
        }

        if (npc is not null)
        {
            PushUserdata(L, npc);
            SetMetatable(L, -1, "Npc");
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }
}