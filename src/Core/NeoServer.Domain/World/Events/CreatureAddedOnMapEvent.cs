using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.World.Events;

/// <summary>
/// Event raised when a walkable creature is added to the map.
/// </summary>
/// <param name="Creature">The creature that was added to the map.</param>
/// <param name="Cylinder">The cylinder containing tile and spectator information.</param>
public record CreatureAddedOnMapEvent(IWalkableCreature Creature, ICylinder Cylinder) : IEvent;
