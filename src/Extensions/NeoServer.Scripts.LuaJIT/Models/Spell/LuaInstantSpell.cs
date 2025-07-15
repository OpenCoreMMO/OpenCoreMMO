namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public class LuaInstantSpell(LuaScriptInterface scriptInterface) : LuaSpell(scriptInterface)
{
    public string Words { get; set; }
    public bool NeedDirection { get; set; }
    public bool HasParams { get; set; }
    public bool NeedCasterTargetOrDirection { get; set; }
}