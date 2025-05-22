using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Scripts.LuaJIT.Models.Callbacks;

namespace NeoServer.Scripts.LuaJIT.Models.Combat;

public class LuaCombat: Script, ILuaCombat
{
    public LuaCombat(LuaScriptInterface scriptInterface) : base(scriptInterface)
    {
    }

    public Dictionary<CombatParam, int> Parameters { get; set; } = new();
    public List<(CallBackType Type, Callback Callback)> Callbacks { get; set; } = new();
    
    public CombatValues CombatValues { get; set; }
    
    public void SetParameter(CombatParam combatParam, int value) => Parameters.TryAdd(combatParam, value);
    
    public void SetCallback(CallBackType callBackType, Callback callback) =>
        Callbacks.Add((callBackType, callback));
    
    public void SetPlayerCombatValues(CombatValues combatValues)
    {
        CombatValues = combatValues;
    }
}

public interface ILuaCombat
{
    Dictionary<CombatParam, int> Parameters { get; set; }
    List<(CallBackType Type, Callback Callback)> Callbacks { get; set; }
    CombatValues CombatValues { get; set; }
    void SetParameter(CombatParam combatParam, int value);
    void SetCallback(CallBackType callBackType, Callback callback);
    void SetPlayerCombatValues(CombatValues combatValues);
}

public struct CombatValues
{
    public CombatFormula FormulaType { get; set; }
    public double MinA { get; set; }
    public double MinB { get; set; }
    public double MaxA { get; set; }
    public double MaxB { get; set; }
}