using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Helpers;

public class DefaultValueJsonConverter<T> : JsonConverter<T> where T : new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize the object as usual
        var instance = JsonSerializer.Deserialize<T>(ref reader, options);

        // Apply [DefaultValue] attributes
        foreach (var property in typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<DefaultValueAttribute>() is { } defaultValue
                && property.CanWrite
                && property.GetValue(instance) == null)
            {
                property.SetValue(instance, defaultValue.Value);
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}