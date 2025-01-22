using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Location.Structs;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureCallback(LuaScriptInterface scriptInterface, ICreature targetCreature, ILogger logger)
{
    private LuaState _luaState;
    private int _params = 0;

    public bool StartScriptInterface(int scriptId)
    {
        if (scriptId == -1)
            return false;

        if (!LuaScriptInterface.ReserveScriptEnv())
        {
            logger.Error(
                "[CreatureCallback::startScriptInterface] - {} {} Call stack overflow. Too many lua script calls being nested.",
                targetCreature.GetType().Name,
                targetCreature.Name
            );
            return false;
        }

        LuaScriptInterface.GetScriptEnv()
            .SetScriptId(scriptId, scriptInterface);

        _luaState = scriptInterface.GetLuaState();

        scriptInterface.PushFunction(scriptId);

        return true;
    }

    public void PushSpecificCreature(ICreature creature)
    {
        if (creature is INpc npc)
            LuaScriptInterface.PushUserdata(_luaState, npc);
        else if (creature is IMonster monster)
            LuaScriptInterface.PushUserdata(_luaState, monster);
        else if (creature is IPlayer player)
            LuaScriptInterface.PushUserdata(_luaState, player);
        else
            return;

        _params++;
        LuaScriptInterface.SetMetatable(_luaState, -1, GetCreatureClass(creature));
    }

    public bool PersistLuaState()
        => _params > 0 && scriptInterface.CallFunction(_params);

    public void PushCreature(ICreature creature)
    {
        _params++;
        LuaScriptInterface.PushUserdata(_luaState, creature);
        LuaScriptInterface.SetCreatureMetatable(_luaState, -1, creature);
    }

    public void PushPosition(Location value)
    {
        _params++;
        LuaScriptInterface.PushPosition(_luaState, value);
    }

    public void PushNumber(int value)
    {
        _params++;
        Lua.PushNumber(_luaState, value);
    }

    public void PushString(string value)
    {
        _params++;
        LuaScriptInterface.PushString(_luaState, value);
    }

    public void PushBoolean(bool value)
    {
	    _params++;
        LuaScriptInterface.PushBoolean(_luaState, value);
    }

    private string GetCreatureClass(ICreature creature)
    {
        if (creature is INpc npc)
            return "Npc";
        else if (creature is IMonster monster)
            return "Monster";
        else if (creature is IPlayer player)
            return "Player";

        return string.Empty;
    }
}