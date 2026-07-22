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

        foreach (var (thing, operationType, stackPosition) in @event.Operations.Operations)
            switch (operationType)
            {
                case Operation.Removed:
                    if (thing is ICumulative cumulativeToRemove)
                    {
                        cumulativeToRemove.OnReduced -= map.OnItemReduced;
                    }

                    eventAggregator.InvokeEvent(new ThingRemovedFromTileEvent(thing,
                        cylinderOperation.Removed(thing, stackPosition)));
                    break;
                case Operation.Updated:
                    if (thing is ICumulative cumulativeToUpdate)
                    {
                        cumulativeToUpdate.OnReduced += map.OnItemReduced;
                    }

                    eventAggregator.InvokeEvent(new ThingUpdatedOnTileEvent(thing,
                        cylinderOperation.Updated(thing, stackPosition)));
                    break;
                case Operation.Added:
                    if (thing is ICumulative cumulativeToAdd)
                    {
                        cumulativeToAdd.OnReduced += map.OnItemReduced;
                    }
                    eventAggregator.InvokeEvent(new ThingAddedToTileEvent(thing,
                        cylinderOperation.Added(thing)));
                    break;
            }
    }
}