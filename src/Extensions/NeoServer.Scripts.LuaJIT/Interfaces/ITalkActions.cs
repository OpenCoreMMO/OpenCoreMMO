using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT;

public interface ITalkActions
{
    public bool CheckWord(IPlayer player, SpeechType type, string words, string word, TalkAction talkActionPtr);

    public TalkActionResultType CheckPlayerCanSayTalkAction(IPlayer player, SpeechType type, string words);

    public bool RegisterLuaEvent(TalkAction talkAction);

    public void Clear();

    public TalkAction GetTalkAction(string word);
}