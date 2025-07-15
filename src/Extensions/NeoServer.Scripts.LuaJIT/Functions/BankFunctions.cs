using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class BankFunctions : LuaScriptInterface, IBankFunctions
{
    private static LuaHelperService _luaHelper;

    public BankFunctions(LuaHelperService luaHelper) : base(nameof(BankFunctions))
    {
        _luaHelper = luaHelper;
    }

    public void Init(LuaState lua)
    {
        RegisterTable(lua, "Bank");
        RegisterMethod(lua, "Bank", "credit", LuaBankCredit);
    }

    private static int LuaBankCredit(LuaState l)
    {
        // Bank.credit(playerOrGuild, amount)
        var bank = GetBank(l, 1);
        if (bank == null)
        {
            ReportError("Bank is nullptr");
            return 1;
        }

        var amount = GetNumber<ulong>(l, 2);

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
            if (guild is null) return null;

            return guild.Bank;
        }

        var player = _luaHelper.GetPlayer(l, arg, true);
        if (player is null) return null;

        return player.Bank;
    }
}