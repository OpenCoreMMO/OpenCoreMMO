using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.World.Events;

public record CreatureMovedInTheMap(IWalkableCreature Creature, ICylinder Cylinder) : IEvent;