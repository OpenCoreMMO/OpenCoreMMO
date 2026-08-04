using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureMoveEvent(
    ICreature Self,
    ICreature Creature,
    Location FromLocation,
    Location ToLocation) : IEvent;
