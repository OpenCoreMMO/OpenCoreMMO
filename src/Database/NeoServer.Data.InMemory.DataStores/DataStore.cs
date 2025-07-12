using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Helpers;
using System.Linq;

namespace NeoServer.Data.InMemory.DataStores;

public class DataStore<TStore, TKey, TValue> : IDataStore<TKey, TValue> where TStore : DataStore<TStore, TKey, TValue>
{
    private static DataStore<TStore, TKey, TValue> _data;

    private readonly Dictionary<TKey, TValue> _values = new();

    public static DataStore<TStore, TKey, TValue> Data
    {
        get
        {
            _data ??= new DataStore<TStore, TKey, TValue>();
            return _data;
        }
    }

    public virtual IEnumerable<TValue> All => _values.Values;
    public virtual IDictionary<TKey, TValue> Map => _values;

    public void Clear()
    {
        _values.Clear();
    }

    public virtual void AddOrUpdate(TKey key, TValue value)
    {
        _values.AddOrUpdate(key, value);
    }

    public void AddOrUpdateRange(IEnumerable<(TKey, TValue)> values)
    {
        foreach (var value in values)
            _values.AddOrUpdate(value.Item1, value.Item2);
    }

    public virtual TValue Get(TKey key)
    {
        return _values.TryGetValue(key, out var value) ? value : default;
    }

    public virtual bool TryGetValue(TKey key, out TValue value)
    {
        if(key is string strKey)
        {
            var actualKey = _values.Keys.FirstOrDefault(k =>
                k is string s && s.Equals(strKey, StringComparison.InvariantCultureIgnoreCase));
            if (actualKey != null)
                return _values.TryGetValue(actualKey, out value);
        }

        return _values.TryGetValue(key, out value);
    }

    public virtual bool Contains(TKey key)
    {
        return _values.ContainsKey(key);
    }
}