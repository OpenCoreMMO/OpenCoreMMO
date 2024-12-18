using LuaNET;

namespace NeoServer.Scripts.LuaJIT;

public class TalkActionFunctions : LuaScriptInterface
{
    public TalkActionFunctions() : base(nameof(TalkActionFunctions))
    {
    }

    public static void Init(LuaState L)
    {
        RegisterSharedClass(L, "TalkAction", "", LuaCreateTalkAction);
        RegisterMethod(L, "TalkAction", "onSay", LuaTalkActionOnSay);
        //RegisterMethod(L, "TalkAction", "groupType", LuaTalkActionGroupType);
        RegisterMethod(L, "TalkAction", "register", LuaTalkActionRegister);
        RegisterMethod(L, "TalkAction", "separator", LuaTalkActionSeparator);
        //RegisterMethod(L, "TalkAction", "getName", LuaTalkActionGetName);
        //RegisterMethod(L, "TalkAction", "getGroupType", LuaTalkActionGetGroupType);
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
        //var talkActionSharedPtr = new TalkAction();
        talkActionSharedPtr.SetWords(wordsVector);

        ////IntPtr unmanagedAddr = Marshal.AllocHGlobal(Marshal.SizeOf(talkActionSharedPtr));
        ////Marshal.StructureToPtr(talkActionSharedPtr, unmanagedAddr, true);

        //var talkAction = new Structs.TalkAction(GetScriptEnv().GetScriptInterface());
        //var talkAction = new Structs.TalkAction();
        //talkAction.SetWords(wordsVector);

        PushUserdata(L, talkActionSharedPtr);
        //PushUserdata(L, talkAction, 1);
        //PushUserdata(L, test, 1);

        //var teste = GetUserdataShared<Teste>(L, 3, 1);

        SetMetatable(L, -1, "TalkAction");
        //SetWeakMetatable(L, -1, "TalkAction");

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

    //private static int LuaTalkActionGroupType(lua_State L)
    //{
    //    // talkAction:groupType(GroupType = GROUP_TYPE_NORMAL)
    //    //var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);
    //    //if (talkActionSharedPtr == null)
    //    //{
    //    //    ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
    //    //    PushBoolean(L, false);
    //    //    return 1;
    //    //}

    //    //var groupType = Account.GroupType.None;

    //    //var type = Lua.Type(L, 2);
    //    //if (type == LuaType.Number)
    //    //{
    //    //    groupType = (Account.GroupType)GetNumber<byte>(L, 2);
    //    //}
    //    //else if (type == LuaType.String)
    //    //{
    //    //    var strValue = GetString(L, 2);
    //    //    switch (strValue.ToLower())
    //    //    {
    //    //        case "normal":
    //    //            groupType = Account.GroupType.Normal;
    //    //            break;
    //    //        case "tutor":
    //    //            groupType = Account.GroupType.Tutor;
    //    //            break;
    //    //        case "seniortutor":
    //    //        case "senior tutor":
    //    //            groupType = Account.GroupType.SeniorTutor;
    //    //            break;
    //    //        case "gamemaster":
    //    //        case "gm":
    //    //            groupType = Account.GroupType.GameMaster;
    //    //            break;
    //    //        case "communitymanager":
    //    //        case "cm":
    //    //        case "community manager":
    //    //            groupType = Account.GroupType.CommunityManager;
    //    //            break;
    //    //        case "god":
    //    //            groupType = Account.GroupType.God;
    //    //            break;
    //    //        default:
    //    //            var errorString = $"Invalid group type string value {strValue} for group type for script: {GetScriptEnv().GetScriptInterface().GetLoadingScriptName()}";
    //    //            ReportError(errorString);
    //    //            PushBoolean(L, false);
    //    //            return 1;
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    var errorString = $"Expected number or string value for group type for script: {GetScriptEnv().GetScriptInterface().GetLoadingScriptName()}";
    //    //    ReportError(errorString);
    //    //    PushBoolean(L, false);
    //    //    return 1;
    //    //}

    //    //talkActionSharedPtr.SetGroupType(groupType);
    //    //PushBoolean(L, true);
    //    return 1;
    //}

    private static int LuaTalkActionRegister(LuaState L)
    {
        // talkAction:register()
        var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);
        if (talkActionSharedPtr == null)
        {
            ReportError(nameof(LuaTalkActionRegister), GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        if (!talkActionSharedPtr.IsLoadedCallback())
        {
            PushBoolean(L, false);
            return 1;
        }

        //if (talkActionSharedPtr.GroupType == Account.GroupType.None)
        //{
        //    var errorString = $"TalkAction with name {talkActionSharedPtr.Words} does not have groupType";
        //    ReportError(errorString);
        //    PushBoolean(L, false);
        //    return 1;
        //}

        PushBoolean(L, TalkActions.GetInstance().RegisterLuaEvent(talkActionSharedPtr));
        //RemoveUserdata<TalkAction>();

        //talkActionSharedPtr.ExecuteSay(Game.GetInstance().GetCurrentPlayer(), "!help", "", SpeakClassesType.TALKTYPE_SAY);

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

    //private static int LuaTalkActionGetGroupType(lua_State L)
    //{
    //    // local groupType = talkAction:getGroupType()
    //    var talkActionSharedPtr = GetUserdata<TalkAction>(L, 1);
    //    if (talkActionSharedPtr == null)
    //    {
    //        ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND));
    //        PushBoolean(L, false);
    //        return 1;
    //    }

    //    lua_pushnumber(L, (double)talkActionSharedPtr.GroupType);
    //    return 1;
    //}
}
