using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Npcs;

[Serializable]
public class NpcJsonData
{
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("walk-interval")] public int WalkInterval { get; set; }
        [JsonPropertyName("health")] public HealthData Health { get; set; }
        [JsonPropertyName("look")] public LookData Look { get; set; }

        [JsonPropertyName("marketings")] public string[] Marketings { get; set; }
        [JsonPropertyName("dialog")]  public DialogData[] Dialog { get; set; }
        [JsonPropertyName("script")] public string Script { get; set; }
        [JsonPropertyName("shop")] public ShopData[] Shop { get; set; }

        [JsonPropertyName("custom-data")] public dynamic CustomData { get; set; }

        [Serializable]
        public class DialogData
        {
            [JsonPropertyName("words")] public string[] Words { get; set; }

            [JsonPropertyName("answers")] public string[] Answers { get; set; }
            [JsonPropertyName("then")] public DialogData[] Then { get; set; }
            [JsonPropertyName("action")] public string Action { get; set; }
            [JsonPropertyName("end")] public bool End { get; set; }

            [JsonPropertyName("store-at")] public string StoreAt { get; set; }

            [JsonPropertyName("back")] public byte Back { get; set; }
        }

        public class HealthData
        {
            [JsonPropertyName("now")] public uint Now { get; set; }
            [JsonPropertyName("max")] public uint Max { get; set; }
        }

        public class LookData
        {
            [JsonPropertyName("type")] public ushort Type { get; set; }
            [JsonPropertyName("corpse")] public ushort Corpse { get; set; }
            [JsonPropertyName("body")] public ushort Body { get; set; }
            [JsonPropertyName("legs")] public ushort Legs { get; set; }
            [JsonPropertyName("feet")] public ushort Feet { get; set; }
            [JsonPropertyName("head")] public ushort Head { get; set; }
            [JsonPropertyName("addons")] public ushort Addons { get; set; }
        }

        [Serializable]
        public class ShopData
        {
            [JsonPropertyName("item")]
            public ushort Item { get; set; }
            
            [JsonPropertyName("sell")]
            public uint Sell { get; set; }
            
            [JsonPropertyName("buy")]
            public uint Buy { get; set; }
        }
}