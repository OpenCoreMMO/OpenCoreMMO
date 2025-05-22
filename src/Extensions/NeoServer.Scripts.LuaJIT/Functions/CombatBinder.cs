using LuaNET;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.DataManagers;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models;
using NeoServer.Scripts.LuaJIT.Models.Callbacks;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using Serilog;
using LuaDataType = NeoServer.Scripts.LuaJIT.Enums.LuaDataType;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CombatBinder() : LuaScriptInterface(nameof(CombatBinder)), ICombatFunctionMapper
{
    public void Init(LuaState lua)
    {
        RegisterSharedClass(lua, "Combat", "", HandleCombatCreate);
        RegisterMetaMethod(lua, "Combat", "__eq", LuaUserdataCompare<ILuaCombat>);

        RegisterMethod(lua, "Combat", "setParameter", HandleSetParameterMethod);
        RegisterMethod(lua, "Combat", "setFormula", HandleSetFormulaMethod);

        RegisterMethod(lua, "Combat", "setArea", HandleNotImplementedMethod);
        RegisterMethod(lua, "Combat", "addCondition", HandleNotImplementedMethod);
        RegisterMethod(lua, "Combat", "setCallback", HandleSetCallbackMethod);
        RegisterMethod(lua, "Combat", "setOrigin", HandleNotImplementedMethod);

        RegisterMethod(lua, "Combat", "execute", HandleExecuteMethod);
    }

    private static int HandleExecuteMethod(LuaState L)
    {
        // combat:execute(creature, variant)
        var combat = GetUserdata<ILuaCombat>(L, 1);
        if (combat is null)
        {
            Lua.PushNil(L);
            return 1;
        }

        if (IsUserdata(L, 2))
        {
            LuaDataType type = GetUserdataType(L, 2);
            if (type != LuaDataType.Player && type != LuaDataType.Monster && type != LuaDataType.Npc)
            {
                PushBoolean(L, false);
                return 1;
            }

            var creature = GetUserdata<ICreature>(L, 2);
            var variant = GetVariant(L, 3);
        }

        //ICreature creature = GetCreature(L, 2);
        Lua.PushNil(L);
        return 1;
    }

    private static int HandleSetCallbackMethod(LuaState lua)
    {
        // combat:setCallback(key, function)
        var combat = GetUserdata<ILuaCombat>(lua, 1);
        if (combat is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        CallBackType key = GetNumber<CallBackType>(lua, 2);
        string callbackName = GetString(lua, 3);

        var callback = new Callback(GetScriptEnv().GetScriptInterface());
        combat.SetCallback(key, callback);

        PushBoolean(lua, callback.LoadCallback(callbackName));
        return 1;
    }

    private static int HandleSetFormulaMethod(LuaState l)
    {
        // combat:setFormula(type, mina, minb, maxa, maxb)
        var combat = GetUserdata<ILuaCombat>(l, 1);
        if (combat is null)
        {
            Lua.PushNil(l);
            return 1;
        }

        var type = GetNumber<CombatFormula>(l, 2);
        var minA = GetNumber<double>(l, 3);
        var minB = GetNumber<double>(l, 4);
        var maxA = GetNumber<double>(l, 5);
        var maxB = GetNumber<double>(l, 6);

        combat.SetPlayerCombatValues(new CombatValues()
        {
            FormulaType = type,
            MinA = minA,
            MinB = minB,
            MaxA = maxA,
            MaxB = maxB
        });

        PushBoolean(l, true);
        return 1;
    }

    private static int HandleSetParameterMethod(LuaState l)
    {
        // combat:setParameter(key, value)
        var combat = GetUserdata<ILuaCombat>(l, 1);
        if (combat is null)
        {
            Lua.PushNil(l);
            return 1;
        }

        CombatParam key = GetNumber<CombatParam>(l, 2);
        int value;
        if (IsBoolean(l, 3))
        {
            value = GetBoolean(l, 3) ? 1 : 0;
        }
        else
        {
            value = GetNumber<int>(l, 3);
        }

        combat.SetParameter(key, value);
        PushBoolean(l, true);
        return 1;
    }


    private static int HandleCombatCreate(LuaState lua)
    {
        //Combat
        var combat = new LuaCombat(GetScriptEnv().GetScriptInterface());

        PushUserdata(lua, combat);
        SetMetatable(lua, -1, "Combat");
        return 1;
    }
}