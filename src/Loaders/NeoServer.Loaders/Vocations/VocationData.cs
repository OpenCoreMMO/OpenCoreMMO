using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Loaders.Converts;

namespace NeoServer.Loaders.Vocations;

public sealed class VocationData : IVocation
{
    public byte Id { get; set; }

    public string Name { get; set; }

    public string FromVoc { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public new ushort GainCap { get; set; }
    
    [JsonConverter(typeof(UshortConverter))]
    public new ushort GainHp { get; set; }
    
    [JsonConverter(typeof(UshortConverter))]
    public ushort GainMana { get; set; }
    
    [JsonConverter(typeof(ByteConverter))]
    public new byte GainHpTicks { get; set; }
    
    [JsonConverter(typeof(ByteConverter))]
    public new byte GainManaTicks { get; set; }
    
    [JsonConverter(typeof(UshortConverter))]
    public new ushort GainHpAmount { get; set; }
    
    [JsonConverter(typeof(UshortConverter))]
    public new ushort GainManaAmount { get; set; }
    
    [JsonConverter(typeof(UshortConverter))]
    public new ushort AttackSpeed { get; set; }

    public string Inspect { get; set; }

    [JsonConverter(typeof(UshortConverter))]
    public new ushort BaseSpeed { get; set; }

    public string Clientid { get; set; }
    public string Description { get; set; }
    public IVocationFormula Formula { get; set; }

    [JsonConverter(typeof(ByteConverter))]
    public new byte SoulMax { get; set; }

    public byte VocationType => Id; //(byte)NeoServer.Game.Common.Creatures.Players.VocationType.None : byte.Parse(Id);

    [JsonConverter(typeof(ByteConverter))]
    public new byte GainSoulTicks { get; set; }
    
    [JsonConverter(typeof(SkillConverter))]
    public new Dictionary<SkillType, float> Skills { get; set; }
}