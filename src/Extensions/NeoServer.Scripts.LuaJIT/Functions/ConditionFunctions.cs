using LuaNET;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
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
    }

    private static int LuaConditionCreate(LuaState luaState)
    {
        // Condition(conditionType, conditionId = CONDITIONID_COMBAT, subid = 0, isPersistent = false)
        var conditionType = GetNumber<ConditionType>(luaState, 2);

        if(conditionType == ConditionType.None)
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
            PushUserdata(luaState, condition is ICondition);
            SetMetatable(luaState, -1, "Condition");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}