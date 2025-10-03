using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Server.Helpers.JsonConverters;

public class AbstractConverter<TReal> : JsonConverter<TReal> where TReal : class
{
    public override TReal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<TReal>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, TReal value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}