namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IDataStore
{
}

public interface IDataStore<TKey, TValue>
{
    IEnumerable<TValue> All { get; }
    IDictionary<TKey, TValue> Map { get; }
    void AddOrUpdate(TKey key, TValue value);
    TValue Get(TKey key);
    bool TryGetValue(TKey key, out TValue value);
    bool Contains(TKey key);
    void Clear();
}