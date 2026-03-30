using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Location;
using NeoServer.Loaders.Helpers;
using Newtonsoft.Json;

namespace NeoServer.Loaders.Spawns;

[Serializable]
public class SpawnData
{
    public ushort CenterX { get; set; }
    public ushort CenterY { get; set; }
    public byte CenterZ { get; set; }

    public byte Radius { get; set; }

    [JsonProperty("Monsters")]
    [JsonConverter(typeof(ArrayOrObjectConverter<Creature>))]
    public IEnumerable<Creature> Monsters { get; set; }

    [JsonProperty("Monster")]
    [JsonConverter(typeof(ArrayOrObjectConverter<Creature>))]
    private IEnumerable<Creature> Monster
    {
        set => Monsters = value;
    }

    [JsonProperty("Npcs")]
    [JsonConverter(typeof(ArrayOrObjectConverter<Creature>))]
    public IEnumerable<Creature> Npcs { get; set; }

    [JsonProperty("Npc")]
    [JsonConverter(typeof(ArrayOrObjectConverter<Creature>))]
    private IEnumerable<Creature> Npc
    {
        set => Npcs = value;
    }

    [Serializable]
    public class Creature
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public byte Z { get; set; }
        public int SpawnTime { get; set; }
        public Direction Direction { get; set; }
    }
}