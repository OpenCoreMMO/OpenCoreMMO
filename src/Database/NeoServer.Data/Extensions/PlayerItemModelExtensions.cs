using System;
using System.Collections.Generic;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Data.Extensions;

public static class PlayerItemModelExtensions
{
    public static Dictionary<ItemTypeAttribute, IConvertible> GetAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var attributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.Count, itemEntity.Amount }
        };

        if (itemEntity.Charges > 0) attributes.Add(ItemTypeAttribute.Charges, itemEntity.Charges);

        if (itemEntity.DecayDuration > 0)
        {
            attributes.Add(ItemTypeAttribute.DecayTo, itemEntity.DecayTo);
            attributes.Add(ItemTypeAttribute.DecayElapsed, itemEntity.DecayElapsed);
            attributes.Add(ItemTypeAttribute.Duration, itemEntity.DecayDuration);
        }

        return attributes;
    }
}