using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class MonsterFunctions : LuaScriptInterface, IMonsterFunctions
{
    private static IGameCreatureManager _gameCreatureManager;

    public MonsterFunctions(IGameCreatureManager gameCreatureManager) : base(nameof(MonsterFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Monster", "Creature", LuaMonsterCreate);
        RegisterMetaMethod(luaState, "Monster", "__eq", LuaUserdataCompare<IMonster>);
    }

    private static int LuaMonsterCreate(LuaState luaState)
    {
        // Monster(id or userdata)
        IMonster monster = null;
        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<uint>(luaState, 2);
            _gameCreatureManager.TryGetCreature(id, out var creature);

            if (creature is IMonster)
            {
                monster = creature as IMonster;
            }
            else
            {
                Lua.PushNil(luaState);
                return 1;
            }
        }
        else if (IsUserdata(luaState, 2))
        {
            if (GetUserdataType(luaState, 2) != LuaDataType.Monster)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            monster = GetUserdata<IMonster>(luaState, 2);
        }
        else
        {
            monster = null;
        }

        if (monster is not null)
        {
            PushUserdata(luaState, monster);
            SetMetatable(luaState, -1, "Monster");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}