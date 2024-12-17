using System.Text.Json;

namespace NeoServer.Loaders.Extensions;

public static class JsonTextExtensions
{
    public static uint GetUInt32FromJson(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => uint.TryParse(element.GetString(), out var value) ? value : 0,
            JsonValueKind.Number => element.GetUInt32(),
            _ => 0
        };
    }

    public static ushort GetUInt16FromJson(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => ushort.TryParse(element.GetString(), out var value) ? value : (ushort)0,
            JsonValueKind.Number => element.GetUInt16(),
            _ => (ushort)0
        };
    }
    
    public static string GetStringFromJson(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            _ => null
        };
    }
}