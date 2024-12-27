using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class MonsterLuaMapping : LuaScriptInterface, IMonsterLuaMapping
{
    private static IGameCreatureManager _gameCreatureManager;

    public MonsterLuaMapping(IGameCreatureManager gameCreatureManager) : base(nameof(MonsterLuaMapping))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Monster", "Creature", LuaMonsterCreate);
        RegisterMetaMethod(L, "Monster", "__eq", LuaUserdataCompare<IMonster>);
    }

    private static int LuaMonsterCreate(LuaState L)
    {
        // Monster(id or userdata)
        IMonster monster = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<uint>(L, 2);
            _gameCreatureManager.TryGetCreature(id, out var creature);

            if (creature != null && creature is IMonster)
            {
                monster = creature as IMonster;
            }
            else
            {
                Lua.PushNil(L);
                return 1;
            }
        }
        else if (IsUserdata(L, 2))
        {
            if (GetUserdataType(L, 2) != LuaDataType.Monster)
            {
                Lua.PushNil(L);
                return 1;
            }
            monster = GetUserdata<IMonster>(L, 2);
        }
        else
        {
            monster = null;
        }

        if (monster is not null)
        {
            PushUserdata(L, monster);
            SetMetatable(L, -1, "Monster");
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }
}
