using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Application.Common.Contracts.Scripts;

public interface ILuaGameManager
{
    bool PlayerSaySpell(IPlayer player, SpeechType type, string text);
}
