using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface ITalkActionScripts
{
    bool PlayerSaySpell(IPlayer player, SpeechType type, string text);
}