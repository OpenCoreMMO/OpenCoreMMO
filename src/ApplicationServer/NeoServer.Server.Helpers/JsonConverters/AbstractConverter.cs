using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Server.Helpers.JsonConverters;

public class AbstractConverter<TReal, TAbstract> : JsonConverter<TAbstract> where TReal : TAbstract
{
    public override TAbstract Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<TReal>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, TAbstract value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (TReal)value, options);
    }
}
