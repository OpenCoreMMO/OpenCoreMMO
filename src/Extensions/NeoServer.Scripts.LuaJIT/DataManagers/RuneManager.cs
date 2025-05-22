using NeoServer.Scripts.LuaJIT.Models.Spell;

namespace NeoServer.Scripts.LuaJIT.DataManagers;

public class RuneManager
{
    private Dictionary<int, LuaRune> AttackRunesIds { get; } = new(); 

    public void Register(LuaRune rune) => AttackRunesIds.Add(rune.RuneId, rune);

    public LuaRune GetRegisteredRune(int clientId)
    {
        AttackRunesIds.TryGetValue(clientId, out var rune);
        return rune;
    }
}