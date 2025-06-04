using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface ITalkActionScriptService
{
    bool Say(IPlayer player, SpeechType type, string text);
}