using System;
using System.Linq;
using System.Text.Json;

namespace NeoServer.Loaders.Items.Parsers;

public static class AttributeValueParser
{
    public static dynamic Parse(object value)
    {
        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.Array } jsonElement => ParseArray(jsonElement),
            JsonElement { ValueKind: JsonValueKind.String } jsonElement => jsonElement.GetString(),
            JsonElement { ValueKind: JsonValueKind.Number } jsonElement => jsonElement.TryGetInt64(out var intValue) 
                ? intValue 
                : jsonElement.GetDouble(),
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            JsonElement { ValueKind: JsonValueKind.Null } => null,
            _ => value
        }; }

    private static dynamic ParseArray(JsonElement jsonArray)
    {
        if (jsonArray.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Input is not a JSON array.");

        var firstElement = jsonArray.EnumerateArray().FirstOrDefault();

        return firstElement.ValueKind switch
        {
            JsonValueKind.True or JsonValueKind.False => 
                jsonArray.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False 
                    ? e.GetBoolean() 
                    : throw new FormatException("Invalid boolean value in JSON array.")).ToArray(),

            JsonValueKind.Number when jsonArray.EnumerateArray().All(e => e.TryGetUInt16(out _)) => 
                jsonArray.EnumerateArray().Select(e => e.GetUInt16()).ToArray(),

            JsonValueKind.Number when jsonArray.EnumerateArray().All(e => e.TryGetUInt32(out _)) => 
                jsonArray.EnumerateArray().Select(e => e.GetUInt32()).ToArray(),

            JsonValueKind.Number when jsonArray.EnumerateArray().All(e => e.TryGetUInt64(out _)) => 
                jsonArray.EnumerateArray().Select(e => e.GetUInt64()).ToArray(),

            JsonValueKind.Number => 
                jsonArray.EnumerateArray().Select(e => e.GetDouble()).ToArray(),

            JsonValueKind.String when jsonArray.EnumerateArray().All(e => DateTime.TryParse(e.GetString(), out _)) => 
                jsonArray.EnumerateArray().Select(e => e.GetDateTime()).ToArray(),

            JsonValueKind.String => 
                jsonArray.EnumerateArray().Select(e => e.GetString()).ToArray(),

            _ => throw new ArgumentException("Invalid JSON array format.")
        };
    }
}