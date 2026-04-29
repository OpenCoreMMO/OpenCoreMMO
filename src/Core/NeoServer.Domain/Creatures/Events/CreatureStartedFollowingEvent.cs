using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureStartedFollowingEvent(
    IWalkableCreature Creature,
    ICreature Following,
    FindPathParams Fpp) : IEvent;
