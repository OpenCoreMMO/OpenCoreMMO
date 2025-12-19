using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.World.Events;

/// <summary>
/// Event raised when a thing (item) on a tile is updated (e.g., stack amount changed).
/// </summary>
/// <param name="Thing">The thing that was updated on the tile.</param>
/// <param name="Cylinder">The cylinder containing tile and spectator information.</param>
public record ThingUpdatedOnTileEvent(IThing Thing, ICylinder Cylinder) : IEvent;
