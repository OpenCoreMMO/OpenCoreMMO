using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Items.Events;

/// <summary>
/// Event raised when a thing (item or creature) is removed from a tile.
/// </summary>
/// <param name="Thing">The thing that was removed from the tile.</param>
/// <param name="Cylinder">The cylinder containing tile and spectator information.</param>
public record ThingRemovedFromTileEvent(IThing Thing, ICylinder Cylinder) : IEvent;
