using System.Buffers;
using System.Globalization;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Items;

public sealed class ItemAttributeList
{
    private readonly IDictionary<ItemAttribute, (dynamic, ItemAttributeList)> _defaultAttributes;
    private IDictionary<string, (dynamic, ItemAttributeList)> customAttributes;

    public ItemAttributeList()
    {
        _defaultAttributes = new Dictionary<ItemAttribute, (dynamic, ItemAttributeList)>();
    }

    private IDictionary<string, (dynamic, ItemAttributeList)> _customAttributes
    {
        get
        {
            customAttributes ??= new Dictionary<string, (dynamic, ItemAttributeList)>(StringComparer
                .InvariantCultureIgnoreCase);
            return customAttributes;
        }
    }

    public void SetCustomAttribute(string attribute, int attributeValue)
    {
        _customAttributes[attribute] = (attributeValue, null);
    }

    public void SetCustomAttribute(string attribute, IConvertible attributeValue)
    {
        _customAttributes[attribute] = (attributeValue, null);
    }

    public void SetCustomAttribute(string attribute, dynamic values)
    {
        _customAttributes[attribute] = (values, null);
    }

    public void SetCustomAttribute(string attribute, IConvertible attributeValue, ItemAttributeList attrs)
    {
        _customAttributes[attribute] = (attributeValue, attrs);
    }

    public void SetAttribute(ItemAttribute attribute, IConvertible attributeValue)
    {
        _defaultAttributes.AddOrUpdate(attribute, (attributeValue, null));
    }

    public void SetAttribute(IDictionary<ItemAttribute, IConvertible> attributeValues)
    {
        if (attributeValues.IsNull()) return;

        foreach (var (key, value) in attributeValues) SetAttribute(key, value);
    }

    public void SetAttribute(ItemAttribute attribute, dynamic values)
    {
        if (attribute is ItemAttribute.ActionId or ItemAttribute.UniqueId) return;
        _defaultAttributes[attribute] = (values, null);
    }

    public void SetAttribute(ItemAttribute attribute, IConvertible attributeValue, ItemAttributeList attrs)
    {
        if (attribute is ItemAttribute.ActionId or ItemAttribute.UniqueId) return;
        _defaultAttributes[attribute] = (attributeValue, attrs);
    }

    public bool HasAttribute(ItemAttribute attribute)
    {
        return _defaultAttributes.ContainsKey(attribute);
    }

    public bool HasAttribute(string attribute)
    {
        return _customAttributes.ContainsKey(attribute);
    }

    public T GetAttribute<T>(ItemAttribute attribute) where T : struct
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value))
            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);

        return default;
    }

    public bool TryGetAttribute<T>(ItemAttribute attribute, out T attrValue) where T : struct
    {
        attrValue = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public bool TryGetAttribute(ItemAttribute attribute, out string attrValue)
    {
        attrValue = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = value.Item1;
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public bool TryGetAttribute<T>(string attribute, out T attrValue)
    {
        attrValue = default;

        if (_customAttributes is null) return false;

        if (!_customAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public string GetAttribute(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public T GetAttribute<T>(string attribute)
    {
        if (_customAttributes is null) return default;


        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (IsNullable(value.Item1)) return (T)value.Item1;

            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }

        return default;
    }

    public string GetAttribute(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public dynamic[] GetAttributeArray(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return default;
        if (value.Item1 is not Array) return new[] { value.Item1 };

        var pool = ArrayPool<dynamic>.Shared;
        dynamic[] newArray = pool.Rent(value.Item1.Length);

        for (var i = 0; i < value.Item1.Length; i++) newArray[i] = value.Item1[i];

        pool.Return(newArray);

        int count = value.Item1.Length;
        return newArray[..count];
    }

    public T[] GetAttributeArray<T>(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return default;

        if (value.Item1 is not Array) return new[] { (T)value.Item1 };

        var pool = ArrayPool<T>.Shared;
        T[] newArray = pool.Rent(value.Item1.Length);

        for (var i = 0; i < value.Item1.Length; i++) newArray[i] = (T)value.Item1[i];

        pool.Return(newArray);

        int count = value.Item1.Length;
        return newArray[..count];
    }

    public dynamic[] GetAttributeArray(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (value.Item1 is not Array) return default;

            var pool = ArrayPool<dynamic>.Shared;
            dynamic[] newArray = pool.Rent(value.Item1.Length);

            for (var i = 0; i < value.Item1.Length; i++) newArray[i] = value.Item1[i];

            pool.Return(newArray);

            int count = value.Item1.Length;
            return newArray[..count];
        }

        return default;
    }

    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>()
    {
        if (_defaultAttributes is null && _customAttributes is null) return default;

        var dictionary = new Dictionary<TKey, TValue>();

        if (_defaultAttributes is not null)
            foreach (var item in _defaultAttributes)
                dictionary.Add((TKey)Convert.ChangeType(item.Key, typeof(TKey), CultureInfo.InvariantCulture),
                    (TValue)item.Value.Item1);
        if (_customAttributes is not null)
            foreach (var item in _customAttributes)
                dictionary.Add((TKey)Convert.ChangeType(item.Key, typeof(TKey), CultureInfo.InvariantCulture),
                    (TValue)item.Value.Item1);
        return dictionary;
    }

    public ItemAttributeList GetInnerAttributes(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return value.Item2;

        return default;
    }

    private static bool IsNullable(dynamic value)
    {
        if (value == null)
            return true; // null itself is always nullable

        var type = ((object)value).GetType();

        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    public ItemAttributeList Clone()
    {
        var clone = new ItemAttributeList();

        foreach (var kvp in _defaultAttributes)
        {
            clone._defaultAttributes[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2?.Clone());
        }

        if (customAttributes != null)
        {
            foreach (var kvp in customAttributes)
            {
                clone._customAttributes[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2?.Clone());
            }
        }

        return clone;
    }
}