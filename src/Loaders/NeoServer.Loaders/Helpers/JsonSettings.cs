using System.Text.Json;
using NeoServer.Loaders.Quest;

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