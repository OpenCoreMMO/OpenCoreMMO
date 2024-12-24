using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class TalkAction : Script, ITalkAction
{
    private string words;
    private string separator = "\"";
    //private account.GroupType groupType = account.GroupType.GROUP_TYPE_NONE;

    public TalkAction(LuaScriptInterface context) : base(context) { }

    public string GetWords()
    {
        return words;
    }

    public void SetWords(List<string> newWords)
    {
        foreach (var word in newWords)
        {
            if (!string.IsNullOrEmpty(words))
            {
                words += ", ";
            }
            words += word;
        }
    }

    public string GetSeparator()
    {
        return separator;
    }

    public void SetSeparator(string sep)
    {
        separator = sep;
    }

    public bool ExecuteSay(IPlayer player, string words, string param, SpeechType type)
    {
        // onSay(player, words, param, type)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            Console.WriteLine($"[TalkAction::ExecuteSay - Player {player.Name} words {GetWords()}] " +
                              $"Call stack overflow. Too many lua script calls being nested. Script name {GetScriptInterface().GetLoadingScriptName()}");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        LuaScriptInterface.PushString(L, words);
        LuaScriptInterface.PushString(L, param);
        //lua_pushnumber(L, (double)type);

        return GetScriptInterface().CallFunction(3);
    }

    //public void SetGroupType(account.GroupType newGroupType)
    //{
    //    groupType = newGroupType;
    //}

    //public account.GroupType GetGroupType()
    //{
    //    return groupType;
    //}

    public override string GetScriptTypeName()
    {
        return "onSay";
    }
}