using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Loaders.Converts;

namespace NeoServer.Loaders.Vocations;

public sealed class VocationData
{
    public byte Id { get; set; }

    public string Name { get; set; }

    [JsonConverter(typeof(ByteConverter))] public byte FromVoc { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort GainCap { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort GainHp { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort GainMana { get; set; }

    [JsonConverter(typeof(ByteConverter))] public byte GainHpTicks { get; set; }

    [JsonConverter(typeof(ByteConverter))] public byte GainManaTicks { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort GainHpAmount { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort GainManaAmount { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort AttackSpeed { get; set; }

    public string Inspect { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public ushort BaseSpeed { get; set; }

    public string Clientid { get; set; }
    public string Description { get; set; }
    public VocationFormula Formula { get; set; }

    [JsonConverter(typeof(ByteConverter))] public byte SoulMax { get; set; }

    public byte VocationType => Id; //(byte)NeoServer.Game.Common.Creatures.Players.VocationType.None : byte.Parse(Id);

    [JsonConverter(typeof(ByteConverter))] public byte GainSoulTicks { get; set; }

    [JsonConverter(typeof(SkillConverter))]
    public Dictionary<SkillType, float> Skills { get; set; }
}