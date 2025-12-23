using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.World.Events;

/// <summary>
///     Event raised when a thing (item) is added to a tile.
/// </summary>
/// <param name="Thing">The thing that was added to the tile.</param>
/// <param name="Cylinder">The cylinder containing tile and spectator information.</param>
public record ThingAddedToTileEvent(IThing Thing, ICylinder Cylinder) : IEvent;