using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface ITalkAction
{
    bool ExecuteSay(IPlayer player, string words, string param, SpeechType type);
}