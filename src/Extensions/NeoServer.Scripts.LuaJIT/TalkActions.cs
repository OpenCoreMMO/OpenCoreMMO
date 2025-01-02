using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT;

public class TalkActions : ITalkActions
{
    #region Members

    private readonly Dictionary<string, TalkAction> _talkActions = new();

    #endregion

    #region Constructors

    public TalkActions()
    {
        _instance = this;
    }

    #endregion

    #region Injection

    private static TalkActions _instance;

    public static TalkActions GetInstance()
    {
        return _instance == null ? _instance = new TalkActions() : _instance;
    }

    #endregion

    #region Public Methods

    public bool CheckWord(IPlayer player, SpeechType type, string words, string word, TalkAction talkActionPtr)
    {
        var spacePos = words.IndexOfAny(new[] { ' ', '\t', '\n', '\r' });
        var firstWord = spacePos != -1 ? words.Substring(0, spacePos) : words;

        // Check for exact equality from saying word and talkaction stored word
        if (firstWord != word) return false;

        //var groupId = player.Group.Id;
        //if (groupId < talkActionPtr.GroupType)
        //{
        //    return false;
        //}

        var param = string.Empty;
        var wordPos = words.IndexOf(word);
        var talkactionLength = word.Length;
        if (wordPos != -1 && wordPos + talkactionLength < words.Length)
        {
            param = words.Substring(wordPos + talkactionLength);
            param = param.TrimStart(' ');
        }

        var separator = talkActionPtr.GetSeparator();
        if (separator != " ")
            if (!string.IsNullOrEmpty(param))
            {
                if (param != separator) return false;

                param = param.Substring(1);
            }

        return talkActionPtr.ExecuteSay(player, words, param, type);
    }

    public TalkActionResultType CheckPlayerCanSayTalkAction(IPlayer player, SpeechType type, string words)
    {
        foreach (var talkActionPair in _talkActions)
        {
            var talkactionWords = talkActionPair.Key;
            var talkActionPtr = talkActionPair.Value;

            if (talkactionWords.Contains(','))
            {
                var wordsList = talkactionWords.Split(',');
                foreach (var word in wordsList)
                    if (CheckWord(player, type, words, word.Trim(), talkActionPtr))
                        return TalkActionResultType.TALKACTION_BREAK;
            }
            else
            {
                if (CheckWord(player, type, words, talkactionWords, talkActionPtr))
                    return TalkActionResultType.TALKACTION_BREAK;
            }
        }

        return TalkActionResultType.TALKACTION_CONTINUE;
    }

    public bool RegisterLuaEvent(TalkAction talkAction)
    {
        if (!_talkActions.ContainsKey(talkAction.GetWords()))
        {
            _talkActions.Add(talkAction.GetWords(), talkAction);
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _talkActions.Clear();
    }

    public TalkAction GetTalkAction(string word)
    {
        _talkActions.TryGetValue(word, out var talkAction);
        return talkAction;
    }

    #endregion
}