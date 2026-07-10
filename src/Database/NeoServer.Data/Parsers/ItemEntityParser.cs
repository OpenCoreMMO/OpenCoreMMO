using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Data.Parsers;

public static class ItemEntityParser
{
    public static T ToPlayerItemEntity<T>(IItem item) where T : PlayerItemBaseEntity, new()
    {
        var itemModel = new T
        {
            ServerId = (short)item.Metadata.ServerId,
            Amount = item is ICumulative cumulative ? cumulative.Amount : (short)1,
            DecayTo = item.Decay?.DecaysTo,
            DecayDuration = item.Decay?.Duration,
            DecayElapsed = item.Decay?.Elapsed,
            Charges = item is IChargeable chargeable ? chargeable.Charges : null,
            Attributes = item.ExtractAllAttributes()
        };

        return itemModel;
    }

    public static IItem BuildContainer<T>(IContainer container, List<T> items, Location location,
        IItemFactory itemFactory) where T : PlayerItemBaseEntity
    {
        if (items == null || items.Count == 0)
            return container;

        var childrenContainers = new Queue<(IContainer Container, int ContainerId)>();
        childrenContainers.Enqueue((container, 0));

        while (childrenContainers.TryDequeue(out var dequeuedContainer))
        {
            var containerItemsRecords = items.Where(x => x.ParentId == dequeuedContainer.ContainerId)
                .OrderByDescending(x => x.Id).ToList();

            foreach (var itemRecord in containerItemsRecords)
            {
                //todo: check this, if need pass Metadata to itemFactory.Create
                var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>();
                if (itemRecord.Charges.HasValue)
                    itemTypeAttributes[ItemTypeAttribute.Charges] = itemRecord.Charges.Value;
                if (itemRecord.DecayElapsed.HasValue && itemRecord.DecayElapsed.Value > 0)
                    itemTypeAttributes[ItemTypeAttribute.DecayElapsed] = itemRecord.DecayElapsed.Value;
                if (itemRecord.DecayDuration.HasValue && itemRecord.DecayDuration.Value > 0)
                    itemTypeAttributes[ItemTypeAttribute.Duration] = itemRecord.DecayDuration.Value;
                if (itemTypeAttributes.Count == 0)
                    itemTypeAttributes = null;

                var item = itemFactory.Create((ushort)itemRecord.ServerId, location, itemTypeAttributes, null,
                    itemRecord.GetAttributes(), itemRecord.GetCustomAttributes());

                if (item is ICumulative cumulativeItem && itemRecord.Amount > 1)
                    cumulativeItem.SetAmount((byte)itemRecord.Amount);

                dequeuedContainer.Container.AddItem(item);

                if (item is not IContainer childContainer)
                    continue;

                childContainer.SetParent(dequeuedContainer.Container);
                childrenContainers.Enqueue((childContainer, itemRecord.ContainerId));
            }
        }

        return container;
    }
}