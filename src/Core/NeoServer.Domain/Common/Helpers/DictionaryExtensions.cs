namespace NeoServer.Domain.Common.Helpers;

public static class DictionaryExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> map)
    {
        public bool AddOrUpdate(TKey key, TValue value)
        {
            map[key] = value;
            return true;
        }

        public bool AddOrUpdate(TKey key,
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
}