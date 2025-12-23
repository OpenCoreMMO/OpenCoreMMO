using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Map;

namespace NeoServer.Server.Events.World;

/// <summary>
///     Handles tile change events (added, removed, updated items) for the map.
///     Manages cumulative item subscriptions and publishes networking events.
/// </summary>
public class TileChangedEventHandler(IMap map, IEventAggregator eventAggregator, CylinderOperation cylinderOperation)
    : IApplicationEventHandler<TileChangedEvent>
{
    public void Handle(TileChangedEvent @event)
    {
        if (@event.Operations?.HasAnyOperation != true) return;

        foreach (var operation in @event.Operations.Operations)
            switch (operation.Item2)
            {
                case Operation.Removed:
                    if (operation.Item1 is ICumulative cumulativeToRemove)
                        cumulativeToRemove.OnReduced -= map.OnItemReduced;
                    eventAggregator.InvokeEvent(new ThingRemovedFromTileEvent(operation.Item1,
                        cylinderOperation.Removed(operation.Item1, operation.Item3)));
                    break;
                case Operation.Updated:
                    if (operation.Item1 is ICumulative cumulativeToUpdate)
                        cumulativeToUpdate.OnReduced += map.OnItemReduced;
                    eventAggregator.InvokeEvent(new ThingUpdatedOnTileEvent(operation.Item1,
                        cylinderOperation.Updated(operation.Item1, operation.Item1.Amount)));
                    break;
                case Operation.Added:
                    if (operation.Item1 is ICumulative cumulativeToAdd) cumulativeToAdd.OnReduced += map.OnItemReduced;
                    eventAggregator.InvokeEvent(new ThingAddedToTileEvent(operation.Item1,
                        cylinderOperation.Added(operation.Item1)));
                    break;
            }
    }
}