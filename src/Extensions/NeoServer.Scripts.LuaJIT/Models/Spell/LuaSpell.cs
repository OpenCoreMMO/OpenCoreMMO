namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class LuaSpell(LuaScriptInterface scriptInterface) : Script(scriptInterface)
{
    public SpellType SpellType { get; set; }
    public int Id { get; set; }
    public SpellGroup PrimaryGroup { get; set; }
    public SpellGroup SecondaryGroup { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int MagicLevel { get; set; }
    public uint Cooldown { get; set; }
    public uint PrimaryGroupCooldown { get; set; }
    public uint SecondaryGroupCooldown { get; set; }
    public bool NeedTarget { get; set; }
    public bool BlockingSolid { get; set; }
    public bool BlockingCreature { get; set; }
}