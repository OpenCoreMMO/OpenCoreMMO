using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.World.Events;

namespace NeoServer.Server.Events.World;

/// <summary>
///     Handles tile loaded events, subscribing to cumulative item reduced events.
/// </summary>
public class TileLoadedEventHandler(IMap map)
    : IApplicationEventHandler<TileLoadedEvent>
{
    public void Handle(TileLoadedEvent @event)
    {
        if (@event.Tile is not IDynamicTile dynamicTile) return;

        foreach (var item in dynamicTile.AllItems)
            if (item is ICumulative cumulative)
                cumulative.OnReduced += map.OnItemReduced;
    }
}