using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Creatures.Models.Bases;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureFunctions : LuaScriptInterface, ICreatureFunctions
{
    private static IGameCreatureManager _gameCreatureManager;

    public CreatureFunctions(IGameCreatureManager gameCreatureManager) : base(nameof(TalkActionFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Creature", "", LuaCreatureCreate);
        RegisterMetaMethod(L, "Creature", "__eq", LuaUserdataCompare<ICreature>);
        RegisterMethod(L, "Creature", "getId", LuaGetId);
        RegisterMethod(L, "Creature", "getName", LuaGetName);
    }

    private static int LuaCreatureCreate(LuaState L)
    {
        // Creature(id or name or userdata)

        ICreature creature = null;
        if (IsNumber(L, 2))
        {
            //creature = Game.GetInstance().GetCreatureByID(GetNumber<int>(L, 2));
            var id = GetNumber<int>(L, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.CreatureId == id);
        }
        else if (IsString(L, 2))
        {
            var name = GetString(L, 2);
            creature = (Creature)_gameCreatureManager.GetCreatures().FirstOrDefault(c => c.Name.Equals(name));
        }
        else if (IsUserdata(L, 2))
        {
            //LuaData_t type = getUserdataType(L, 2);
            //if (type != LuaData_t::Player && type != LuaData_t::Monster && type != LuaData_t::Npc)
            //{
            //    lua_pushnil(L);
            //    return 1;
            //}
            //creature = getUserdataShared<Creature>(L, 2);
        }
        else
        {
            creature = null;
        }

        if (creature != null)
        {
            PushUserdata(L, creature);
            SetCreatureMetatable(L, -1, creature);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGetId(LuaState L)
    {
        // creature:getId()
        var creature = GetUserdata<ICreature>(L, 1);

        if (creature != null)
        {
            Lua.PushNumber(L, creature.CreatureId);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaGetName(LuaState L)
    {
        // player:getName()
        var creature = GetUserdata<ICreature>(L, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            PushBoolean(L, false);
            return 0;
        }

        PushString(L, creature.Name);
        return 1;
    }
}
