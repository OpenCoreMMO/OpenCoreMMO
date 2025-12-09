using System.Text.Json;

namespace NeoServer.Loaders.Helpers;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultBufferSize = 4096,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    static JsonSettings()
    {
    }
}