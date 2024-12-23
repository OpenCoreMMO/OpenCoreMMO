using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Converts;

public class UshortNullableConverter : JsonConverter<ushort?>
{
    public override ushort? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (ushort.TryParse(reader.GetString(), out var value))
            {
                return value;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetUInt16();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, ushort? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}