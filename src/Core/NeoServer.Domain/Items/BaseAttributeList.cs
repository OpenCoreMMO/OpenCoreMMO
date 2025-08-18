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

    protected static bool IsNullable(dynamic value)
    {
        if (value == null)
            return true; // null itself is always nullable

        var type = ((object)value).GetType();

        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    private static TKey ConvertKey<TKey>(object key)
    {
        if (typeof(TKey).IsEnum)
        {
            if (key is string s)
                return (TKey)Enum.Parse(typeof(TKey), s, true);
            return (TKey)Enum.ToObject(typeof(TKey), key);
        }

        return (TKey)Convert.ChangeType(key, typeof(TKey), CultureInfo.InvariantCulture);
    }

    #region Attributes

    public bool HasAttribute(T attribute)
    {
        return _defaultAttributes.ContainsKey(attribute);
    }

    public TValue GetAttribute<TValue>(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value))
        {
            if (value.Item1 is TValue tValue)
                return tValue;

            if (typeof(IConvertible).IsAssignableFrom(typeof(TValue)))
                return (TValue)Convert.ChangeType(value.Item1, typeof(TValue), CultureInfo.InvariantCulture);

            return (TValue)value.Item1;
        }

        return default;
    }

    public string GetAttribute(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

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

    public void RemoveAttribute(T attribute)
    {
        if (_defaultAttributes is null) return;
        _defaultAttributes.Remove(attribute);
    }

    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>()
    {
        if (_defaultAttributes is null) return default;

        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var item in _defaultAttributes)
        {
            var key = ConvertKey<TKey>(item.Key);
            dictionary[key] = (TValue)item.Value.Item1;
        }

        return dictionary;
    }

    public BaseAttributeList<T> GetInnerAttributes(T attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return value.Item2;

        return default;
    }

    public bool TryGetValue(T attribute, out dynamic value)
    {
        value = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var attr)) return false;

        value = attr.Item1;
        return true;
    }

    #endregion

    #region Custom Attributes

    public bool HasCustomAttribute(string attribute)
    {
        return _customAttributes.ContainsKey(attribute);
    }

    public TValue GetCustomAttribute<TValue>(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (value.Item1 is TValue tValue)
                return tValue;

            if (typeof(IConvertible).IsAssignableFrom(typeof(TValue)))
                return (TValue)Convert.ChangeType(value.Item1, typeof(TValue), CultureInfo.InvariantCulture);

            return (TValue)value.Item1;
        }

        return default;
    }

    public string GetCustomAttribute(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public bool TryGetCustomAttribute<TValue>(string attribute, out TValue attrValue)
    {
        attrValue = default;

        if (_customAttributes is null) return false;

        if (!_customAttributes.TryGetValue(attribute, out var value)) return false;

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

    public dynamic[] GetCustomAttributeArray(string attribute)
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

    public void SetCustomAttribute(IDictionary<string, IConvertible> attributeValues)
    {
        if (attributeValues.IsNull()) return;
        foreach (var (key, value) in attributeValues) SetCustomAttribute(key, value);
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

    public void SetCustomAttribute(string attribute, IConvertible attributeValue, BaseAttributeList<T> attrs)
    {
        _customAttributes[attribute] = (attributeValue, attrs);
    }

    public void RemoveCustomAttribute(string attribute)
    {
        if (_customAttributes is null) return;
        _customAttributes.Remove(attribute);
    }

    public Dictionary<TKey, TValue> ToDictionaryCustom<TKey, TValue>()
    {
        if (_customAttributes is null) return default;

        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var item in _customAttributes)
        {
            var key = ConvertKey<TKey>(item.Key);
            dictionary[key] = (TValue)item.Value.Item1;
        }

        return dictionary;
    }

    #endregion

    public BaseAttributeList<T> Clone()
    {
        var clone = new BaseAttributeList<T>();

        if (_defaultAttributes != null)
        {
            foreach (var kv in _defaultAttributes)
            {
                var valueCopy = CloneValue(kv.Value.Item1);
                var innerCopy = kv.Value.Item2 != null ? kv.Value.Item2.Clone() : null;
                clone._defaultAttributes[kv.Key] = (valueCopy, innerCopy);
            }
        }

        if (customAttributes != null)
        {
            clone.customAttributes = new Dictionary<string, (dynamic, BaseAttributeList<T>)>(
                StringComparer.InvariantCultureIgnoreCase);

            foreach (var kv in customAttributes)
            {
                var valueCopy = CloneValue(kv.Value.Item1);
                var innerCopy = kv.Value.Item2 != null ? kv.Value.Item2.Clone() : null;
                clone.customAttributes[kv.Key] = (valueCopy, innerCopy);
            }
        }

        return clone;
    }

    private static dynamic CloneValue(dynamic value)
    {
        if (value is null) return null;

        object obj = value;

        if (obj is Array arr)
        {
            var elementType = obj.GetType().GetElementType() ?? typeof(object);
            var copy = Array.CreateInstance(elementType, arr.Length);
            Array.Copy(arr, copy, arr.Length);
            return copy;
        }

        if (obj is ICloneable cloneable)
            return cloneable.Clone();

        return value;
    }
}