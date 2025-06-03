using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Sound;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureSayEventHandler : IGameEventHandler
{
    private readonly IMap map;

    public CreatureSayEventHandler(IMap map)
    {
        this.map = map;
    }

    public void Execute(ICreature creature, SpeechType speechType, string message, ICreature receiver = null)
    {
        if (creature is null) return;

        if (receiver is ISociableCreature sociableCreature)
        {
            sociableCreature.Hear(creature, speechType, message);
            return;
        }

        foreach (var spectator in map.GetCreaturesAtPositionZone(creature.Location))
        {
            if (!SoundRuleValidator.ShouldHear(creature, spectator, speechType)) continue;

            if (spectator is ISociableCreature listener)
                listener.Hear(creature, speechType, message);
        }
    }
}