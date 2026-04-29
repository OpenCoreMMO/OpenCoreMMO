using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureMovedEvent(
    IWalkableCreature Creature,
    Location FromLocation,
    Location ToLocation,
    ICylinderSpectator[] Spectators) : IEvent;
