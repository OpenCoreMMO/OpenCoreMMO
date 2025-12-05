using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Creatures.Player.Vocation;

public class Vocation
{
    public static float DefaultSkillMultiplier = 4;
    public byte Id { get; set; }
    public string Clientid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ushort GainCap { get; set; }
    public ushort GainHp { get; set; }
    public ushort GainMana { get; set; }
    public byte GainHpTicks { get; set; }
    public byte GainManaTicks { get; set; }
    public ushort GainHpAmount { get; set; }
    public ushort GainManaAmount { get; set; }
    public ushort AttackSpeed { get; set; }
    public string Inspect { get; set; }
    public ushort BaseSpeed { get; set; }
    public byte SoulMax { get; set; }
    public byte GainSoulTicks { get; set; }
    public byte FromVoc { get; set; }
    public VocationFormula Formula { get; set; }
    public Dictionary<SkillType, float> Skills { get; set; }
    public byte VocationType => Id;
    public bool IsPromotion => Id != FromVoc && Id > 0;
    public string InspectText => string.IsNullOrWhiteSpace(Inspect) ? $"is {Description.ToLower()}" : Inspect;
    public byte BaseId => FromVoc == 0 ? Id : FromVoc;
}