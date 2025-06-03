using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface ITalkAction
{
    bool ExecuteSay(IPlayer player, string words, string param, SpeechType type);
}