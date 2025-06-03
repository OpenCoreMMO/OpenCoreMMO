using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Scripts.LuaJIT.Models.Callbacks;
using NeoServer.Scripts.LuaJIT.Parsers;

namespace NeoServer.Scripts.LuaJIT.Models.Combat;

public class LuaCombat : Script
{
    public LuaCombat(LuaScriptInterface scriptInterface) : base(scriptInterface)
    {
    }

    public Dictionary<CombatParam, int> Parameters { get; set; } = new();
    public (CallBackType Type, Callbacks.Callback Callback) Callback { get; set; }
    public CombatValues CombatValues { get; set; }

    public void SetParameter(CombatParam combatParam, int value)
    {
        Parameters.TryAdd(combatParam, value);
    }

    public Callback SetCallback(CallBackType callBackType)
    {
        var callback = callBackType switch
        {
            CallBackType.LevelMagicValue or CallBackType.SkillValue => new ValueCallback(_scriptInterface)
            {
                Formula = callBackType
            },
            _ => throw new ArgumentOutOfRangeException(nameof(callBackType), callBackType, null)
        };

        Callback = (callBackType, callback);
        return callback;
    }

    public void SetPlayerCombatValues(CombatValues combatValues)
    {
        CombatValues = combatValues;
    }

    public CombatParameter BuildCombatParameter(IPlayer player)
    {
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_TYPE, out var combatType);
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_EFFECT, out var effect);
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_DISTANCEEFFECT, out var shootType);

        var callback = Callback.Type switch
        {
            CallBackType.LevelMagicValue => Callback.Callback as ValueCallback,
            _ => throw new ArgumentOutOfRangeException()
        };

        var damageValues = callback.GetMinMaxValues(player);

        return new CombatParameter
        {
            DamageType = ((CombatType)combatType).ToDamageType(),
            Effect = (EffectT)effect,
            ShootType = (Domain.Common.Item.ShootType)shootType,
            MinDamage = (ushort)damageValues.Min,
            MaxDamage = (ushort)damageValues.Max,
            Range = 7
        };
    }
}

public struct CombatValues
{
    public CombatFormula CombatFormula { get; set; }
    public double MinA { get; set; }
    public double MinB { get; set; }
    public double MaxA { get; set; }
    public double MaxB { get; set; }
}