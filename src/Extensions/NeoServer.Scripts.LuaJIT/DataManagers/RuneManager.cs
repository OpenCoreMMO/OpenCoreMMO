using NeoServer.Scripts.LuaJIT.Models.Spell;

namespace NeoServer.Scripts.LuaJIT.DataManagers;

public class RuneManager
{
    private Dictionary<int, LuaRune> AttackRunes { get; } = new(); 

    public void Register(LuaRune rune) => AttackRunes.TryAdd(rune.RuneId, rune);

    public bool IsRegistered(int clientId) => AttackRunes.ContainsKey(clientId);

    public LuaRune GetRegisteredRune(int clientId)
    {
        AttackRunes.TryGetValue(clientId, out var rune);
        return rune;
    }

    public void Clear() => AttackRunes.Clear();
}