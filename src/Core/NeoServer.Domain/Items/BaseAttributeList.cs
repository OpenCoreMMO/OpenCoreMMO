using System.Buffers;
using System.Globalization;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Items;

public class BaseAttributeList<T> where T : Enum
{
    protected readonly IDictionary<T, (dynamic, BaseAttributeList<T>)> _defaultAttributes;
    protected IDictionary<string, (dynamic, BaseAttributeList<T>)> customAttributes;

    public BaseAttributeList()
    {
        _defaultAttributes = new Dictionary<T, (dynamic, BaseAttributeList<T>)>();
    }

    protected IDictionary<string, (dynamic, BaseAttributeList<T>)> _customAttributes 
        => customAttributes ??= new Dictionary<string, (dynamic, BaseAttributeList<T>)>(StringComparer
                .InvariantCultureIgnoreCase);

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

    public void SetCustomAttribute(string attribute, IConvertible attributeValue, BaseAttributeList<T> attrs)
    {
        _customAttributes[attribute] = (attributeValue, attrs);
    }

    public void SetAttribute(T attribute, IConvertible attributeValue)
    {
        _defaultAttributes.AddOrUpdate(attribute, (attributeValue, null));
    }

    public void SetAttribute(IDictionary<T, IConvertible> attributeValues)
    {
        if (attributeValues.IsNull()) return;

        foreach (var (key, value) in attributeValues) SetAttribute(key, value);
    }

    public void SetAttribute(T attribute, dynamic values)
    {
        _defaultAttributes[attribute] = (values, null);
    }

    public void SetAttribute(T attribute, IConvertible attributeValue, BaseAttributeList<T> attrs)
    {
        _defaultAttributes[attribute] = (attributeValue, attrs);
    }

    public bool HasAttribute(T attribute)
    {
        return _defaultAttributes.ContainsKey(attribute);
    }

    public bool HasAttribute(string attribute)
    {
        return _customAttributes.ContainsKey(attribute);
    }

    public TValue GetAttribute<TValue>(T attribute) where TValue : struct
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value))
            return (TValue)Convert.ChangeType(value.Item1, typeof(TValue), CultureInfo.InvariantCulture);

        return default;
    }

    public bool TryGetAttribute<TValue>(T attribute, out TValue attrValue) where TValue : struct
    {
        attrValue = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (TValue)Convert.ChangeType(value.Item1, typeof(TValue), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public bool TryGetAttribute(T attribute, out string attrValue)
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

    public bool TryGetAttribute<TValue>(string attribute, out TValue attrValue)
    {
        attrValue = default;

        if (_customAttributes is null) return false;

        if (!_customAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (TValue)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public string GetAttribute(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public TValue GetAttribute<TValue>(string attribute)
    {
        if (_customAttributes is null) return default;


        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (IsNullable(value.Item1)) return (TValue)value.Item1;

            return (TValue)Convert.ChangeType(value.Item1, typeof(TValue), CultureInfo.InvariantCulture);
        }

        return default;
    }

    public string GetAttribute(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public dynamic[] GetAttributeArray(T attribute)
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

    public TValue[] GetAttributeArray<TValue>(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return default;

        if (value.Item1 is not Array) return new[] { (TValue)value.Item1 };

        var pool = ArrayPool<TValue>.Shared;
        TValue[] newArray = pool.Rent(value.Item1.Length);

        for (var i = 0; i < value.Item1.Length; i++) newArray[i] = (TValue)value.Item1[i];

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

    public BaseAttributeList<T> GetInnerAttributes(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return value.Item2;

        return default;
    }

    protected static bool IsNullable(dynamic value)
    {
        if (value == null)
            return true; // null itself is always nullable

        var type = ((object)value).GetType();

        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}