using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Converts;

public class NumberToStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32().ToString();
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? throw new JsonException("String value is null.");
        }
        throw new JsonException("Invalid JSON format for a string representation of a number.");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (int.TryParse(value, out int number))
        {
            writer.WriteNumberValue(number);
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
