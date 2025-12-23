using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.World.Events;

/// <summary>
/// Event raised when a tile is loaded into the game world.
/// </summary>
/// <param name="Tile">The tile that was loaded.</param>
public record TileLoadedEvent(ITile Tile) : IEvent;
