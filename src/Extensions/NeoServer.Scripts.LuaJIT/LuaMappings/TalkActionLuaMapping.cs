using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class TalkActionLuaMapping : LuaScriptInterface, ITalkActionLuaMapping
{
    public TalkActionLuaMapping() : base(nameof(TalkActionLuaMapping))
    {
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "TalkAction", "", LuaCreateTalkAction);
        RegisterMethod(L, "TalkAction", "onSay", LuaTalkActionOnSay);
        RegisterMethod(L, "TalkAction", "register", LuaTalkActionRegister);
        RegisterMethod(L, "TalkAction", "separator", LuaTalkActionSeparator);
    }

    private static int LuaCreateTalkAction(LuaState L)
    {
        // TalkAction(words) or TalkAction(word1, word2, word3)
        var wordsVector = new List<string>();
        for (var i = 2; i <= Lua.GetTop(L); i++)
        {
            wordsVector.Add(GetString(L, i));
        }

        var talkActionSharedPtr = new TalkAction(GetScriptEnv().GetScriptInterface());
        talkActionSharedPtr.SetWords(wordsVector);

        PushUserdata(L, talkActionSharedPtr);
        SetMetatable(L, -1, "TalkAction");

        return 1;
    }

    private static int LuaTalkActionOnSay(LuaState L)
    {
        // talkAction:onSay(callback)

        var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);

        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionOnSay), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        if (!talkActionSharedPtr.LoadCallback())
        {
            PushBoolean(L, false);
            return 1;
        }

        PushBoolean(L, true);
        return 1;
    }

  
    private static int LuaTalkActionRegister(LuaState L)
    {
        // talkAction:register()
        var talkAction = GetUserdata<TalkAction>(L, 1);
        if (talkAction == null)
        {
            ReportError(nameof(LuaTalkActionRegister), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        if (!talkAction.IsLoadedCallback())
        {
            PushBoolean(L, false);
            return 1;
        }

        //todo: implement this
        //if (talkAction.GroupType == Account.GroupType.None)
        //{
        //    var errorString = $"TalkAction with name {talkActionSharedPtr.Words} does not have groupType";
        //    ReportError(errorString);
        //    PushBoolean(L, false);
        //    return 1;
        //}

        PushBoolean(L, TalkActions.GetInstance().RegisterLuaEvent(talkAction));

        return 1;
    }

    private static int LuaTalkActionSeparator(LuaState L)
    {
        // talkAction:separator(sep)
        var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);
        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionSeparator), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        talkActionSharedPtr.SetSeparator(GetString(L, 2));
        PushBoolean(L, true);
        return 1;
    }

    private static int LuaTalkActionGetName(LuaState L)
    {
        // local name = talkAction:getName()
        var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);
        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionGetName), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        PushString(L, talkActionSharedPtr.GetWords());
        return 1;
    }
}
