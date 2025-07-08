using NeoServer.Scripts.LuaJIT.Models.Spell;

namespace NeoServer.Scripts.LuaJIT.DataManagers;

public class InstantSpellManager
{
    private Dictionary<int, LuaSpell> AttackSpells { get; } = new();

    public void Register(LuaSpell spell)
    {
        AttackSpells.TryAdd(spell.Id, spell);
    }

    public bool IsRegistered(int clientId)
    {
        return AttackSpells.ContainsKey(clientId);
    }

    public LuaSpell GetRegistered(int clientId)
    {
        AttackSpells.TryGetValue(clientId, out var spell);
        return spell;
    }

    public void Clear()
    {
        AttackSpells.Clear();
    }
}