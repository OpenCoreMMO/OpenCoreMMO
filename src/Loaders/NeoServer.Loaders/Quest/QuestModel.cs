using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Quest;

public class QuestModel
{
    [JsonPropertyName("aid")] public ushort ActionId { get; set; }
    [JsonPropertyName("uid")] public uint UniqueId { get; set; }
    [JsonPropertyName("script")] public string Script { get; set; }
    [JsonPropertyName("rewards")] public List<Reward> Rewards { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("group")] public string Group { get; set; }
    [JsonPropertyName("group-key")] public string GroupKey { get; set; }

    [JsonPropertyName("auto-load")]
    [DefaultValue(true)]
    public bool AutoLoad { get; set; }

    public class Reward
    {
        [JsonPropertyName("id")] public ushort ItemId { get; set; }

        [JsonPropertyName("amount")] public byte Amount { get; set; }

        [JsonPropertyName("items")] public List<Reward> Children { get; set; }
    }
}