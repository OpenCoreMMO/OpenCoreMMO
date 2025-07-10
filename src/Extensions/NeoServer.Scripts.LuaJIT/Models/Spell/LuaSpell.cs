using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public abstract class LuaSpell(LuaScriptInterface scriptInterface) : Script(scriptInterface)
{
    public SpellType SpellType { get; set; }
    public int Id { get; set; }
    public SpellGroup PrimaryGroup { get; set; }
    public SpellGroup SecondaryGroup { get; set; }
    public string Name { get; set; }
    public ushort Level { get; set; }
    public ushort MagicLevel { get; set; }
    public ushort Mana { get; set; }
    public ushort ManaPercent { get; set; }
    public ushort Soul { get; set; }
    public byte? Range { get; set; }
    public uint Cooldown { get; set; }
    public uint PrimaryGroupCooldown { get; set; }
    public uint SecondaryGroupCooldown { get; set; }
    public bool NeedTarget { get; set; }
    public bool NeedWeapon { get; set; }
    public bool BlockingSolid { get; set; }
    public bool BlockingCreature { get; set; }
    public bool IsEnabled { get; set; }
    public bool NeedLearn { get; set; }
    public bool IsSelfTarget { get; set; }
    public bool IsPremium { get; set; }
    public bool IsAggressive { get; set; }
    public bool IsLockedPz  { get; set; }
    public byte[] VocationIds { get; set; }
}