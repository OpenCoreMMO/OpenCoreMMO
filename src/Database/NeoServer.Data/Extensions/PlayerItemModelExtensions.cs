using System;
using System.Collections.Generic;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Data.Extensions;

public static class PlayerItemModelExtensions
{
    public static Dictionary<ItemAttribute, IConvertible> GetAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var attributes = new Dictionary<ItemAttribute, IConvertible>
        {
            { ItemAttribute.Count, itemEntity.Amount }
        };

        if (itemEntity.Charges > 0) attributes.Add(ItemAttribute.Charges, itemEntity.Charges);

        if (itemEntity.DecayDuration > 0)
        {
            attributes.Add(ItemAttribute.DecayTo, itemEntity.DecayTo);
            attributes.Add(ItemAttribute.DecayElapsed, itemEntity.DecayElapsed);
            attributes.Add(ItemAttribute.Duration, itemEntity.DecayDuration);
        }

        return attributes;
    }
}