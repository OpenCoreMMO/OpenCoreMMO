using NeoServer.Data.Entities;
using System.Collections.Generic;
using System;
using NeoServer.Domain.Common.Contracts.Items;

public static class PlayerItemModelExtensions
{
    public static Dictionary<ItemAttribute, IConvertible> GetAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var attributes = new Dictionary<ItemAttribute, IConvertible>
        {
            { ItemAttribute.Count, itemEntity.Amount }
        };

        if (itemEntity.Charges > 0)
            attributes[ItemAttribute.Charges] = itemEntity.Charges;

        if (itemEntity.DecayDuration > 0)
        {
            attributes[ItemAttribute.DecayTo] = itemEntity.DecayTo;
            attributes[ItemAttribute.DecayElapsed] = itemEntity.DecayElapsed;
            attributes[ItemAttribute.Duration] = itemEntity.DecayDuration;
        }

        if (itemEntity.Attributes != null)
        {
            foreach (var (attr, value) in itemEntity.Attributes)
            {
                if (attributes.ContainsKey(attr)) continue;
                attributes[attr] = ParseValue(value);
            }
        }

        return attributes;
    }

    public static Dictionary<string, IConvertible> GetCustomAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var customAttributes = new Dictionary<string, IConvertible>();

        if (itemEntity.CustomAttributes != null)
        {
            foreach (var (key, value) in itemEntity.CustomAttributes)
            {
                if (customAttributes.ContainsKey(key)) continue;
                customAttributes[key] = ParseValue(value);
            }
        }

        return customAttributes;
    }

    public static Dictionary<ItemAttribute, IConvertible> GetAttributes(this PlayerInventoryItemEntity itemEntity)
    {
        var attributes = new Dictionary<ItemAttribute, IConvertible>
        {
        };

        if (itemEntity.Attributes != null)
        {
            foreach (var (attr, value) in itemEntity.Attributes)
            {
                if (attributes.ContainsKey(attr)) continue;
                attributes[attr] = ParseValue(value);
            }
        }

        return attributes;
    }

    public static Dictionary<string, IConvertible> GetCustomAttributes(this PlayerInventoryItemEntity itemEntity)
    {
        var customAttributes = new Dictionary<string, IConvertible>();

        if (itemEntity.CustomAttributes != null)
        {
            foreach (var (key, value) in itemEntity.CustomAttributes)
            {
                if (customAttributes.ContainsKey(key)) continue;
                customAttributes[key] = ParseValue(value);
            }
        }

        return customAttributes;
    }

    private static IConvertible ParseValue(string value)
    {
        if (bool.TryParse(value, out var boolVal)) return boolVal;
        if (byte.TryParse(value, out var byteVal)) return byteVal;
        if (ushort.TryParse(value, out var ushortVal)) return ushortVal;
        if (int.TryParse(value, out var intVal)) return intVal;
        if (long.TryParse(value, out var longVal)) return longVal;
        if (double.TryParse(value, out var doubleVal)) return doubleVal;

        return value;
    }

    public static Dictionary<ItemAttribute, string> ExtractAttributes(this IItem item)
    {
        var dict = new Dictionary<ItemAttribute, string>();

        if (item?.Metadata.Attributes == null)
            return dict;

        foreach (var (key, value) in item.Metadata.Attributes.ToDictionary<ItemAttribute, object>())
        {
            if (key is ItemAttribute.Count or ItemAttribute.Charges or ItemAttribute.DecayTo or ItemAttribute.Duration or ItemAttribute.DecayElapsed)
                continue;

            dict[key] = value?.ToString() ?? string.Empty;
        }

        return dict;
    }

    public static Dictionary<string, string> ExtractCustomAttributes(this IItem item)
    {
        var dict = new Dictionary<string, string>();

        if (item?.Metadata.Attributes == null)
            return dict;

        foreach (var (key, value) in item.Metadata.Attributes.ToDictionaryCustom<string, object>())
        {
            dict[key] = value?.ToString() ?? string.Empty;
        }

        return dict;
    }
}
