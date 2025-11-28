using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureSayEventHandler(IMap map) : IGameEventHandler
{
    public void Execute(ICreature creature, SpeechType speechType, string message, ICreature receiver = null)
    {
        if (creature is null) return;

        if (receiver is ISociableCreature sociableCreature)
        {
            sociableCreature.Hear(creature, speechType, message);
            return;
        }

        var (maxDistanceX, maxDistanceY) = speechType switch
        {
            SpeechType.Yell or SpeechType.MonsterYell => ((int)MapViewPort.MaxClientViewPortX * 2 + 2,
                (int)MapViewPort.MaxClientViewPortY * 2 + 2),
            SpeechType.Whisper => (1, 1), // Adjacent squares only for whisper
            _ => ((int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortY)
        };

        var multiFloor = speechType is SpeechType.Yell or SpeechType.MonsterYell;

        foreach (var spectator in map.GetSpectators(creature.Location, multiFloor, true, maxDistanceX, maxDistanceX,
                     maxDistanceY, maxDistanceY))
            if (spectator is ISociableCreature listener)
                listener.Hear(creature, speechType, message);
    }
}