using System.Text.Json.Serialization;

namespace NeoServer.Loaders.TileRule;

public class TileJsonData
{
    [JsonPropertyName("tiles")] public ushort[][] Locations { get; set; }

    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool RequiresPremium { get; set; }
    public string Message { get; set; }
}