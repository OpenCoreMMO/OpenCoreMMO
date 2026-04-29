using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureTurnedToDirectionEvent(
    IWalkableCreature Creature,
    Direction Direction) : IEvent;
