using System.Text.Json;

namespace NeoServer.Loaders.Helpers;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    static JsonSettings()
    {
    }
}