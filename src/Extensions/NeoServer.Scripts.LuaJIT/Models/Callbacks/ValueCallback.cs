using LuaNET;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT.Models.Callbacks;

public class ValueCallback(LuaScriptInterface scriptInterface) : Callback(scriptInterface)
{
    public required CallBackType Formula { get; init; }

    public MinMax GetMinMaxValues(IPlayer player)
    {
        //onGetPlayerMinMaxValues(...)

        if (!GetScriptInterface().InternalReserveScriptEnv()) return MinMax.Zero;

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), scriptInterface);

        var luaState = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaFunctionsLoader.PushUserdata(luaState, player);
        LuaFunctionsLoader.SetMetatable(luaState, -1, "Player");

        var numberOfParameters = 1;
        switch (Formula)
        {
            case CallBackType.LevelMagicValue:
            {
                Lua.PushNumber(luaState, player.Level);
                Lua.PushNumber(luaState, player.MagicLevel);
                numberOfParameters += 2;
                break;
            }
            default:
                LuaFunctionsLoader.ResetScriptEnv();
                throw new ArgumentOutOfRangeException();
        }

        var size0 = Lua.GetTop(luaState);

        if (Lua.PCall(luaState, numberOfParameters, 2, 0) != 0)
        {
            LuaFunctionsLoader.ReportError(null, LuaFunctionsLoader.PopString(luaState));
            LuaFunctionsLoader.ResetScriptEnv();
            return MinMax.Zero;
        }

        var damage = new MinMax(Math.Abs(LuaFunctionsLoader.GetNumber<int>(luaState, -2)),
            Math.Abs(LuaFunctionsLoader.GetNumber<int>(luaState, -1)));
        Lua.Pop(luaState, 2);

        if (Lua.GetTop(luaState) + numberOfParameters + 1 != size0)
            LuaFunctionsLoader.ReportError(null, "Stack size changed!");

        LuaFunctionsLoader.ResetScriptEnv();
        return damage;
    }
}