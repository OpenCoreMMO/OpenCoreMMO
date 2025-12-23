using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.World.Events;

/// <summary>
/// Event raised when items on a tile change (added, removed, or updated).
/// </summary>
/// <param name="Tile">The tile where the change occurred.</param>
/// <param name="Item">The item that was changed.</param>
/// <param name="Operations">The list of operations performed on the tile.</param>
public record TileChangedEvent(ITile Tile, IItem Item, OperationResultList<IItem> Operations) : IEvent;
