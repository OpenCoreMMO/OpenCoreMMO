using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NeoServer.Server.Helpers.Extensions;

public static class DictionaryExtensions
{
    public static bool TryGetValue<T>(this Dictionary<string, object> dictionary, string key, out T value)
    {
        if (dictionary is null || key is null || !dictionary.TryGetValue(key, out var val))
        {
            value = default;
            return false;
        }

        try
        {
            if (val is JsonElement jsonElement && typeof(T).FullName != typeof(JsonElement).FullName)
            {
                val = jsonElement.ValueKind switch
                {
                    JsonValueKind.String=> jsonElement.GetString(),
                    JsonValueKind.False => false,
                    JsonValueKind.True => true,
                    JsonValueKind.Number => jsonElement.GetDecimal(),
                    //JsonValueKind.Object=> throw new Exception("Object not supported"),
                    _ => throw new Exception("Type not supported")
                };
            }
            
            value = (T)Convert.ChangeType(val, typeof(T));
            return true;
        }
        catch
        {
            value = default;
        }

        return false;
    }
}