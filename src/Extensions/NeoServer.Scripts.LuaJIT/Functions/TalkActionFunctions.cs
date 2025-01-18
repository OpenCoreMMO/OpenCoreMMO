using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class TalkActionFunctions : LuaScriptInterface, ITalkActionFunctions
{
    private static ILogger _logger;
    public TalkActionFunctions(ILogger logger) : base(nameof(TalkActionFunctions))
    {
        _logger = logger;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "TalkAction", "", LuaCreateTalkAction);
        RegisterMethod(luaState, "TalkAction", "onSay", LuaTalkActionOnSay);
        RegisterMethod(luaState, "TalkAction", "register", LuaTalkActionRegister);
        RegisterMethod(luaState, "TalkAction", "separator", LuaTalkActionSeparator);
    }

    private static int LuaCreateTalkAction(LuaState luaState)
    {
        // TalkAction(words) or TalkAction(word1, word2, word3)
        var wordsVector = new List<string>();
        for (var i = 2; i <= Lua.GetTop(luaState); i++) wordsVector.Add(GetString(luaState, i));

        var talkActionSharedPtr = new TalkAction(GetScriptEnv().GetScriptInterface(), _logger);
        talkActionSharedPtr.SetWords(wordsVector);

        PushUserdata(luaState, talkActionSharedPtr);
        SetMetatable(luaState, -1, "TalkAction");

        return 1;
    }

    private static int LuaTalkActionOnSay(LuaState luaState)
    {
        // talkAction:onSay(callback)

        var talkActionSharedPtr = GetUserdata<TalkAction>(luaState, 1);

        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionOnSay), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
            return 1;
        }

        if (!talkActionSharedPtr.LoadCallback())
        {
            PushBoolean(luaState, false);
            return 1;
        }

        PushBoolean(luaState, true);
        return 1;
    }


    private static int LuaTalkActionRegister(LuaState luaState)
    {
        // talkAction:register()
        var talkAction = GetUserdata<TalkAction>(luaState, 1);
        if (talkAction == null)
        {
            ReportError(nameof(LuaTalkActionRegister), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
            return 1;
        }

        if (!talkAction.IsLoadedCallback())
        {
            PushBoolean(luaState, false);
            return 1;
        }

        //todo: implement this
        //if (talkAction.GroupType == Account.GroupType.None)
        //{
        //    var errorString = $"TalkAction with name {talkActionSharedPtr.Words} does not have groupType";
        //    ReportError(errorString);
        //    PushBoolean(luaState, false);
        //    return 1;
        //}

        PushBoolean(luaState, TalkActions.GetInstance().RegisterLuaEvent(talkAction));

        return 1;
    }

    private static int LuaTalkActionSeparator(LuaState luaState)
    {
        // talkAction:separator(sep)
        var talkActionSharedPtr = GetUserdata<TalkAction>(luaState, 1);
        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionSeparator), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
            return 1;
        }

        talkActionSharedPtr.SetSeparator(GetString(luaState, 2));
        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaTalkActionGetName(LuaState luaState)
    {
        // local name = talkAction:getName()
        var talkActionSharedPtr = GetUserdata<TalkAction>(luaState, 1);
        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(luaState, false);
            return 1;
        }

        PushString(luaState, talkActionSharedPtr.GetWords());
        return 1;
    }
}