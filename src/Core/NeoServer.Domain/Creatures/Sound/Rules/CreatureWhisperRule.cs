using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Sound.Rules;

internal class CreatureWhisperRule : ISoundRule
{
    public bool IsApplicable(SpeechType type)
    {
        return type is SpeechType.Whisper;
    }

    public bool IsSatisfied(ICreature creature, SpeechType type, ICreature spectator)
    {
        if (!creature.Location.SameFloorAs(spectator.Location)) return false;

        if (creature.Location.GetSqmDistance(spectator.Location) > 1) return false;

        return true;
    }
}