using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Creatures.Common;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class BankFunctionBinder : LuaScriptInterface,IBankFunctionBinder
{
    private static LuaHelperService _luaHelper;

    public BankFunctionBinder(LuaHelperService luaHelper) : base(nameof(BankFunctionBinder))
    {
        _luaHelper = luaHelper;
    }

    public void Init(LuaState lua)
    {
        RegisterTable(lua, "Bank");
        RegisterMethod(lua, "Bank", "credit", HandleCreditFunction);
    }

    private static int HandleCreditFunction(LuaState l)
    {
        var bank = GetBank(l, 1);
        if (bank == null)
        {
            ReportError("Bank is nullptr");
            return 1;
        }

        ulong amount = GetNumber<ulong>(l, 2);

        bank.Credit(amount);
        PushBoolean(l, true);
        return 1;
    }

    private static IBank GetBank(LuaState l, int arg, bool isGuild = false)
    {
        if (GetUserdataType(l, arg) == LuaDataType.Guild)
        {
            var guild = _luaHelper.GetGuild(l, 1);
            return guild.Bank;
        }

        if (isGuild)
        {
            var guild = _luaHelper.GetGuild(l, arg, true);
            if (guild is null)
            {
                return null;
            }

            return guild.Bank;
        }

        var player = _luaHelper.GetPlayer(l, arg, true);
        if (player is null)
        {
            return null;
        }

        return player.Bank;
    }
}

public interface IBankFunctionBinder
{
    void Init(LuaState lua);
}