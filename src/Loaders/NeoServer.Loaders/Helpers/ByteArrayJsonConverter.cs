using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Helpers;

public class ByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Ensure the JSON token is a start of an array
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected a JSON array for byte[].");
        }

        var bytes = new System.Collections.Generic.List<byte>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.Number || !reader.TryGetByte(out var value))
            {
                throw new JsonException("Invalid value in JSON array. Expected byte values (0-255).");
            }

            bytes.Add(value);
        }

        return bytes.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var b in value)
        {
            writer.WriteNumberValue(b);
        }
        writer.WriteEndArray();
    }
}