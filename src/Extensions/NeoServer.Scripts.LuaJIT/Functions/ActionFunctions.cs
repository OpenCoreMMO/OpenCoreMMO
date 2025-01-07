using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ActionFunctions : LuaScriptInterface, IActionFunctions
{
    private static IActions _actions;

    public ActionFunctions(IActions actions) : base(nameof(ActionFunctions))
    {
        _actions = actions;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Action", "", LuaCreateAction);
        RegisterMethod(luaState, "Action", "onUse", LuaActionOnUse);
        RegisterMethod(luaState, "Action", "register", LuaActionRegister);
        RegisterMethod(luaState, "Action", "id", LuaActionItemId);
        RegisterMethod(luaState, "Action", "aid", LuaActionItemActionId);
        RegisterMethod(luaState, "Action", "uid", LuaActionItemUniqueId);
        RegisterMethod(luaState, "Action", "allowFarUse", LuaActionAllowFarUse);
    }

    public static int LuaCreateAction(LuaState luaState)
    {
        // Action()
        var action = new Action(GetScriptEnv().GetScriptInterface());
        PushUserdata(luaState, action);
        SetMetatable(luaState, -1, "Action");
        return 1;
    }

    public static int LuaActionOnUse(LuaState luaState)
    {
        // action:onUse(callback)
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            if (!action.LoadCallback())
            {
                PushBoolean(luaState, false);
                return 1;
            }

            action.SetLoadedCallback(true);
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionOnUse), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }

        return 1;
    }

    public static int LuaActionRegister(LuaState luaState)
    {
        // action:register()
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            if (!action.IsLoadedCallback())
            {
                PushBoolean(luaState, false);
                return 1;
            }

            PushBoolean(luaState, _actions.RegisterLuaEvent(action));
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionRegister), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }

        return 1;
    }

    public static int LuaActionItemId(LuaState luaState)
    {
        // action:id(ids)
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            var parameters = Lua.GetTop(luaState) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    action.SetItemIdsVector(GetNumber<ushort>(luaState, 2 + i));
            else
                action.SetItemIdsVector(GetNumber<ushort>(luaState, 2));
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionItemId), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }
        return 1;
    }

    public static int LuaActionItemActionId(LuaState luaState)
    {
        // action:aid(ids)
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            var parameters = Lua.GetTop(luaState) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    action.SetActionIdsVector(GetNumber<ushort>(luaState, 2 + i));
            else
                action.SetActionIdsVector(GetNumber<ushort>(luaState, 2));
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionItemId), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }
        return 1;
    }

    public static int LuaActionItemUniqueId(LuaState luaState)
    {
        // action:uid(ids)
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            var parameters = Lua.GetTop(luaState) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    action.SetUniqueIdsVector(GetNumber<ushort>(luaState, 2 + i));
            else
                action.SetUniqueIdsVector(GetNumber<ushort>(luaState, 2));
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionItemId), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }
        return 1;
    }

    public static int LuaActionAllowFarUse(LuaState luaState)
    {
        // action:allowFarUse(bool)
        var action = GetUserdata<Action>(luaState, 1);
        if (action != null)
        {
            action.AllowFarUse = GetBoolean(luaState, 2);
            PushBoolean(luaState, true);
        }
        else
        {
            ReportError(nameof(LuaActionItemId), GetErrorDesc(ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
        }

        return 1;
    }
}