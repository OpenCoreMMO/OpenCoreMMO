namespace NeoServer.Domain.Common.Helpers;

public static class DictionaryExtensions
{
    public static bool AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key, TValue value)
    {
        map[key] = value;
        return true;
    }

    public static bool AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key,
        Func<TValue, TValue> funcValue)
    {
        if (map.TryGetValue(key, out var currentValue))
        {
            map[key] = funcValue.Invoke(currentValue);
            return true;
        }

        return map.TryAdd(key, funcValue.Invoke(currentValue));
    }
}