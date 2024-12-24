using LuaNET;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ActionFunctions : LuaScriptInterface, IActionFunctions
{
    private static IActions _actions;

    public ActionFunctions(
        ILuaEnvironment luaEnvironment,
        ILogger logger,
        IActions actions) : base(nameof(ActionFunctions))
    {
        _actions = actions;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Action", "", LuaCreateAction);
        RegisterMethod(L, "Action", "onUse", LuaActionOnUse);
        RegisterMethod(L, "Action", "register", LuaActionRegister);
        RegisterMethod(L, "Action", "id", LuaActionItemId);
    }

    public static int LuaCreateAction(LuaState L)
    {
        // Action()
        var action = new Action(GetScriptEnv().GetScriptInterface());
        PushUserdata(L, action);
        SetMetatable(L, -1, "Action");
        return 1;
    }

    public static int LuaActionOnUse(LuaState L)
    {
        // action:onUse(callback)
        var action = GetUserdata<Action>(L, 1);
        if (action != null)
        {
            if (!action.LoadCallback())
            {
                PushBoolean(L, false);
                return 1;
            }

            action.SetLoadedCallback(true);
            PushBoolean(L, true);
        }
        else
        {
            ReportError(nameof(LuaActionOnUse), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(L, false);
        }
        return 1;
    }

    public static int LuaActionRegister(LuaState L)
    {
        // action:register()
        var action = GetUserdata<Action>(L, 1);
        if (action != null)
        {
            if (!action.IsLoadedCallback())
            {
                PushBoolean(L, false);
                return 1;
            }

            PushBoolean(L, _actions.RegisterLuaEvent(action));
            PushBoolean(L, true);
        }
        else
        {
            ReportError(nameof(LuaActionRegister), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(L, false);
        }
        return 1;
    }

    public static int LuaActionItemId(LuaState L)
    {
        // action:id(ids)
        var action = GetUserdata<Action>(L, 1);
        if (action != null)
        {
            int parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
            {
                for (int i = 0; i < parameters; ++i)
                {
                    action.SetItemIdsVector(GetNumber<ushort>(L, 2 + i));
                }
            }
            else
            {
                action.SetItemIdsVector(GetNumber<ushort>(L, 2));
            }
            PushBoolean(L, true);
        }
        else
        {
            ReportError(nameof(LuaActionItemId), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(L, false);
        }
        return 1;
    }
}
