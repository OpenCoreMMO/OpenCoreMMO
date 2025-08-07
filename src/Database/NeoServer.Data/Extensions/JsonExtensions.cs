using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NeoServer.Data.Extensions;

public static class JsonExtensions
{
    public static string SerializeAttributes<T>(Dictionary<T, string> dict) where T : Enum
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<T, string> DeserializeAttributes<T>(string json) where T : Enum
    {
        return JsonSerializer.Deserialize<Dictionary<T, string>>(json);
    }

    public static string SerializeCustomAttributes(Dictionary<string, string> dict)
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<string, string> DeserializeCustomAttributes(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    public static string SerializeAllAttributes(Dictionary<string, string> dict)
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<string, string> DeserializeAllAttributes(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }
}