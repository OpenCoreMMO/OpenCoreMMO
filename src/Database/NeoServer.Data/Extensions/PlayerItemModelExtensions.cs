using NeoServer.Data.Entities;
using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Items;

public static class PlayerItemModelExtensions
{
    public static Dictionary<ItemAttribute, IConvertible> GetAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var attributes = new Dictionary<ItemAttribute, IConvertible>();

        if (itemEntity.Attributes != null)
        {
            foreach (var (key, value) in itemEntity.Attributes)
            {
                if (Enum.TryParse<ItemAttribute>(key, true, out var attr) && !attributes.ContainsKey(attr))
                    attributes[attr] = ParseValue(value);
            }
        }

        return attributes;
    }

    public static Dictionary<string, IConvertible> GetCustomAttributes(this PlayerItemBaseEntity itemEntity)
    {
        var customAttributes = new Dictionary<string, IConvertible>();

        if (itemEntity.Attributes != null)
        {
            foreach (var (key, value) in itemEntity.Attributes)
            {
                if (!Enum.TryParse<ItemAttribute>(key, true, out _) && !customAttributes.ContainsKey(key))
                    customAttributes[key] = ParseValue(value);
            }
        }

        return customAttributes;
    }

    public static Dictionary<ItemAttribute, IConvertible> GetAttributes(this PlayerInventoryItemEntity itemEntity)
    {
        var attributes = new Dictionary<ItemAttribute, IConvertible>();

        if (itemEntity.Attributes != null)
        {
            foreach (var (key, value) in itemEntity.Attributes)
            {
                if (Enum.TryParse<ItemAttribute>(key, true, out var attr) && !attributes.ContainsKey(attr))
                    attributes[attr] = ParseValue(value);
            }
        }

        return attributes;
    }

    public static Dictionary<string, IConvertible> GetCustomAttributes(this PlayerInventoryItemEntity itemEntity)
    {
        var customAttributes = new Dictionary<string, IConvertible>();

        if (itemEntity.Attributes != null)
        {
            foreach (var (key, value) in itemEntity.Attributes)
            {
                if (!Enum.TryParse<ItemAttribute>(key, true, out _) && !customAttributes.ContainsKey(key))
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

    public static Dictionary<string, string> ExtractAllAttributes(this IItem item)
    {
        var dict = new Dictionary<string, string>();

        if (item?.Attributes == null)
            return dict;

        foreach (var (key, value) in item.Attributes.ToDictionary<ItemAttribute, object>())
            dict[key.ToString()] = value?.ToString() ?? string.Empty;

        foreach (var (key, value) in item.Attributes.ToDictionaryCustom<string, object>())
            dict[key] = value?.ToString() ?? string.Empty;

        return dict;
    }

    public static void LoadAttributes(this PlayerItemBaseEntity entity, IItem item)
    {
        if (entity.Attributes == null)
            return;

        foreach (var (key, value) in entity.Attributes)
        {
            if (Enum.TryParse(typeof(ItemAttribute), key, true, out var attrEnum))
                item.Attributes.SetAttribute((ItemAttribute)attrEnum, value);
            else
                item.Attributes.SetCustomAttribute(key, value);
        }
    }
}