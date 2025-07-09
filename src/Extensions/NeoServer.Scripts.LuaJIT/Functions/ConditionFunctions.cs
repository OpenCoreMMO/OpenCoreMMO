using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ConditionFunctions : LuaScriptInterface, IConditionFunctions
{
    public ConditionFunctions() : base(nameof(ConditionFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Condition", "", LuaConditionCreate);
        RegisterMetaMethod(luaState, "Condition", "__eq", LuaUserdataCompare<ICondition>);

        RegisterMethod(luaState, "Condition", "setParameter", LuaSetParameter);
        RegisterMethod(luaState, "Condition", "setFormula", LuaSetFormula);

    }

    private static int LuaConditionCreate(LuaState luaState)
    {
        // Condition(conditionType, conditionId = CONDITIONID_COMBAT, subid = 0, isPersistent = false)
        var conditionType = GetNumber<ConditionType>(luaState, 2);

        if (conditionType == ConditionType.None)
        {
            ReportError("Invalid condition type");
            return 1;
        }

        var conditionId = GetNumber<int>(luaState, 3);
        var subId = GetNumber<int>(luaState, 4);
        var isPersistent = GetBoolean(luaState, 5);

        //todo: implement conditionId, subid and isPersistent
        var condition = new Condition(conditionType);
        if (condition != null)
        {
            PushUserdata(luaState, (ICondition)condition);
            SetMetatable(luaState, -1, "Condition");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaSetParameter(LuaState luaState)
    {
        // combat:setParameter(key, value)
        var condition = GetUserdata<ICondition>(luaState, 1);
        if (condition is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var key = GetNumber<ConditionParamType>(luaState, 2);
        uint value;
        if (IsBoolean(luaState, 3))
            value = (uint)(GetBoolean(luaState, 3) ? 1 : 0);
        else
            value = GetNumber<uint>(luaState, 3);

        condition.Parameters.TryAdd(key, value);
        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaSetFormula(LuaState luaState)
    {
        // combat:setFormula(mina, minb, maxa, maxb)
        var condition = GetUserdata<ICondition>(luaState, 1);
        if (condition is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var minA = GetNumber<double>(luaState, 2);
        var minB = GetNumber<double>(luaState, 3);
        var maxA = GetNumber<double>(luaState, 4);
        var maxB = GetNumber<double>(luaState, 5);

        condition.FormulaValues = new FormulaValues
        {
            MinA = minA,
            MinB = minB,
            MaxA = maxA,
            MaxB = maxB
        };

        PushBoolean(luaState, true);
        return 1;
    }
}