using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Sound.Rules;

internal interface ISoundRule
{
    bool IsApplicable(SpeechType type);
    bool IsSatisfied(ICreature creature, SpeechType type, ICreature spectator);
}