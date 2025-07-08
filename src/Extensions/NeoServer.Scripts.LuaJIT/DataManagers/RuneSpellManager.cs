using NeoServer.Scripts.LuaJIT.Models.Spell;

namespace NeoServer.Scripts.LuaJIT.DataManagers;

public class RuneSpellManager
{
    private Dictionary<int, LuaRuneSpell> AttackRunes { get; } = new();

    public void Register(LuaRuneSpell rune)
    {
        AttackRunes.TryAdd(rune.RuneId, rune);
    }

    public bool IsRegistered(int clientId)
    {
        return AttackRunes.ContainsKey(clientId);
    }

    public LuaRuneSpell GetRegisteredRune(int clientId)
    {
        AttackRunes.TryGetValue(clientId, out var rune);
        return rune;
    }

    public void Clear()
    {
        AttackRunes.Clear();
    }
}