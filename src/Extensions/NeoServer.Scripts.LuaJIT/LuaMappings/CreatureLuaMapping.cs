using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class CreatureLuaMapping : LuaScriptInterface, ICreatureLuaMapping
{
    private static IGameCreatureManager _gameCreatureManager;

    public CreatureLuaMapping(IGameCreatureManager gameCreatureManager) : base(nameof(CreatureLuaMapping))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Creature", "", LuaCreatureCreate);
        RegisterMetaMethod(L, "Creature", "__eq", LuaUserdataCompare<ICreature>);

        RegisterMethod(L, "Creature", "getId", LuaGetId);
        RegisterMethod(L, "Creature", "getName", LuaGetName);
        RegisterMethod(L, "Creature", "getPosition", LuaCreatureGetPosition);
        RegisterMethod(L, "Creature", "getDirection", LuaCreatureGetDirection);
    }

    private static int LuaCreatureCreate(LuaState L)
    {
        // Creature(id or name or userdata)

        ICreature creature = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<int>(L, 2);
            creature = _gameCreatureManager.GetCreatures().FirstOrDefault(c => c.CreatureId == id);
        }
        else if (IsString(L, 2))
        {
            var name = GetString(L, 2);
            creature = (ICreature)_gameCreatureManager.GetCreatures().FirstOrDefault(c => c.Name.Equals(name));
        }
        else if (IsUserdata(L, 2))
        {
            var type = GetUserdataType(L, 2);
            if (type != LuaDataType.Player && type != LuaDataType.Monster && type != LuaDataType.Npc)
            {
                Lua.PushNil(L);
                return 1;
            }
            creature = GetUserdata<ICreature>(L, 2);
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
            Lua.PushNumber(L, creature.CreatureId);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaGetName(LuaState L)
    {
        // creature:getName()
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

    private static int LuaCreatureGetPosition(LuaState L)
    {
        // creature:getPosition()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            PushPosition(L, creature.Location);
        else
            Lua.PushNil(L);
        return 1;
    }

    private static int LuaCreatureGetDirection(LuaState L)
    {
        // creature:getDirection()
        var creature = GetUserdata<ICreature>(L, 1);
        if (creature != null)
            Lua.PushNumber(L, (byte)creature.Direction);
        else
            Lua.PushNil(L);
        return 1;
    }
}
