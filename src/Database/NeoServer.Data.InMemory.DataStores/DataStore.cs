using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.DataStores;

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

    public virtual void Add(TKey key, TValue value)
    {
        _values.TryAdd(key, value);
    }

    public virtual TValue Get(TKey key)
    {
        return _values.TryGetValue(key, out var value) ? value : default;
    }

    public virtual bool TryGetValue(TKey key, out TValue value)
    {
        return _values.TryGetValue(key, out value);
    }

    public virtual bool Contains(TKey key)
    {
        return _values.ContainsKey(key);
    }
}