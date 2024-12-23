using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoServer.Loaders.Converts;

namespace NeoServer.Loaders.Monsters;

public class MonsterData
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("nameDescription")] public string NameDescription { get; set; }

    [JsonPropertyName("race")] public string Race { get; set; }

    [JsonPropertyName("experience")] public uint Experience { get; set; }

    [JsonPropertyName("speed")] public ushort Speed { get; set; }

    [JsonPropertyName("manacost")] public ushort Manacost { get; set; }

    [JsonPropertyName("health")] public HealthData Health { get; set; }

    [JsonPropertyName("look")] public LookData Look { get; set; }

    [JsonPropertyName("targetchange")] public TargetchangeData Targetchange { get; set; }

    [JsonPropertyName("strategy")] public StrategyData Strategy { get; set; }

    [JsonPropertyName("flags")] public IDictionary<string, ushort> Flags { get; set; }

    [JsonPropertyName("attacks")] public List<Dictionary<string, object>> Attacks { get; set; } = new();

    [JsonPropertyName("defenses")] public List<Dictionary<string, object>> Defenses { get; set; } = new();

    [JsonPropertyName("defense")] public DefenseData Defense { get; set; }

    [JsonPropertyName("elements")] public Dictionary<string, sbyte> Elements { get; set; }

    [JsonPropertyName("immunities")] public Dictionary<string, byte> Immunities { get; set; }

    [JsonPropertyName("voices")] public VoicesData Voices { get; set; }

    [JsonPropertyName("summon")] public SummonData Summon { get; set; }

    [JsonPropertyName("loot")] public List<LootData> Loot { get; set; }

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

    public class TargetchangeData
    {
        [JsonPropertyName("interval")] [JsonConverter(typeof(NumberToStringConverter))] public string Interval { get; set; }

        [JsonPropertyName("chance")] [JsonConverter(typeof(NumberToStringConverter))] public string Chance { get; set; }
    }

    public class StrategyData
    {
        [JsonPropertyName("attack")]  [JsonConverter(typeof(NumberToStringConverter))] public string Attack { get; set; }

        [JsonPropertyName("defense")] [JsonConverter(typeof(NumberToStringConverter))] public string Defense { get; set; }
    }

    public class DefenseData
    {
        [JsonPropertyName("armor")] [JsonConverter(typeof(NumberToStringConverter))] public string Armor { get; set; }

        [JsonPropertyName("defense")] [JsonConverter(typeof(NumberToStringConverter))] public string Defense { get; set; }
    }

    public class Voice
    {
        [JsonPropertyName("sentence")] public string Sentence { get; set; }

        [JsonPropertyName("yell")] public bool Yell { get; set; }
    }

    public class VoicesData
    {
        [JsonPropertyName("interval")] [JsonConverter(typeof(NumberToStringConverter))] public string Interval { get; set; }

        [JsonPropertyName("chance")] [JsonConverter(typeof(NumberToStringConverter))] public string Chance { get; set; }

        [JsonPropertyName("sentences")] public List<Voice> Sentences { get; set; }
    }

    public class ItemData
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("countmax")] public string Countmax { get; set; }

        [JsonPropertyName("chance")] public string Chance { get; set; }

        [JsonPropertyName("item")] public List<ItemData> Item { get; set; }
    }

    public class LootData
    {
        [JsonPropertyName("id")] [JsonConverter(typeof(NumberToStringConverter))]  public string Id { get; set; }

        [JsonPropertyName("countmax")] [JsonConverter(typeof(NumberToStringConverter))]  public string Countmax { get; set; }

        [JsonPropertyName("chance")] [JsonConverter(typeof(NumberToStringConverter))]  public string Chance { get; set; }

        [JsonPropertyName("items")] public List<LootData> Items { get; set; }
    }

    public class SummonData
    {
        [JsonPropertyName("maxSummons")] public int MaxSummons { get; set; }

        [JsonPropertyName("summons")] public List<MonsterSummonData> Summons { get; set; }
    }

    public class MonsterSummonData
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("interval")] public uint Interval { get; set; }

        [JsonPropertyName("chance")] public int Chance { get; set; }

        [JsonPropertyName("max")] public int Max { get; set; }
    }
}