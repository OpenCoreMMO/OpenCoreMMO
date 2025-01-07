using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoServer.Loaders.Helpers;

namespace NeoServer.Loaders.Chats;

[Serializable]
public class ChannelModel
{
    public ushort Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Opened { get; set; }

    [JsonPropertyName("allowed-vocations")]
    [JsonConverter(typeof(ByteArrayJsonConverter))]
    public byte[] Vocations { get; set; }

    [JsonPropertyName("allowed-levels")] public LevelModel Level { get; set; }

    [JsonPropertyName("write-rules")] public WriteModel Write { get; set; }

    public ColorModel Color { get; set; }

    [JsonPropertyName("mute-rule")] public MuteRuleModel MuteRule { get; set; }

    public string Script { get; set; }

    public class LevelModel
    {
        [JsonPropertyName("bigger-than")] public ushort BiggerThan { get; set; }

        [JsonPropertyName("lower-than")] public ushort LowerThan { get; set; }
    }

    public class WriteModel
    {
        [JsonPropertyName("allowed-vocations")]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] Vocations { get; set; }

        [JsonPropertyName("allowed-levels")] public LevelModel Level { get; set; }
    }

    [Serializable]
    public class ColorModel
    {
        public string Default { get; set; }

        [JsonPropertyName("by-vocation")] public Dictionary<int, string> ByVocation { get; set; }
    }

    public class MuteRuleModel
    {
        [JsonPropertyName("messages-count")] public ushort MessagesCount { get; set; }

        [JsonPropertyName("time-to-block")] public ushort TimeToBlock { get; set; }

        [JsonPropertyName("wait-time")] public ushort WaitTime { get; set; }

        [JsonPropertyName("time-multiplier")] public double TimeMultiplier { get; set; }

        [JsonPropertyName("cancel-message")] public string CancelMessage { get; set; }
    }
}