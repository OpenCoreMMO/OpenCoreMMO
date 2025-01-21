using System.Collections.Generic;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IVocation
{
    ushort AttackSpeed { get; set; }
    string Inspect { get; set; }
    ushort BaseSpeed { get; set; }
    string Clientid { get; set; }
    string Description { get; set; }
    IVocationFormula Formula { get; set; }
    string FromVoc { get; set; }
    ushort GainCap { get; set; }
    ushort GainHp { get; set; }
    ushort GainHpAmount { get; set; }
    ushort GainManaAmount { get; set; }
    byte GainHpTicks { get; set; }
    ushort GainMana { get; set; }
    byte GainManaTicks { get; set; }
    byte GainSoulTicks { get; set; }
    byte Id { get; set; }
    string Name { get; set; }
    public Dictionary<SkillType, float> Skills { get; set; }
    byte SoulMax { get; set; }
    byte VocationType { get; }

    public string InspectText => string.IsNullOrWhiteSpace(Inspect) ? $"is {Description.ToLower()}" : Inspect;
}