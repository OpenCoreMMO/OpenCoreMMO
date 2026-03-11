using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.World.Events;

public record ItemMovedToTrashHolder(IItem Item, ITile Tile): IEvent;