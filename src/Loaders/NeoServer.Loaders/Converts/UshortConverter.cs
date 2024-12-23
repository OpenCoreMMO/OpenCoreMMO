using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Converts;
public class UshortConverter : JsonConverter<ushort>
{
    public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String when ushort.TryParse(reader.GetString(), out var value) => value,
            JsonTokenType.Number => reader.GetUInt16(),
            _ => 0
        };
    }

    public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}