using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;
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
    public FormulaValues FormulaValues { get; set; }
    public List<ICondition> Conditions { get; set; } = new();

    public Dictionary<Direction, byte[,]> Areas { get; set; } = new();

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

    public CombatParameter BuildCombatParameter(IPlayer player, IThing target)
    {
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_TYPE, out var combatType);
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_EFFECT, out var effect);
        Parameters.TryGetValue(CombatParam.COMBAT_PARAM_DISTANCEEFFECT, out var shootType);

        var damageValues = new MinMax(0, 0);

        if (Callback.Callback != null)
        {
            var callback = Callback.Type switch
            {
                CallBackType.LevelMagicValue => Callback.Callback as ValueCallback,
                CallBackType.SkillValue => Callback.Callback as ValueCallback,
                _ => throw new ArgumentOutOfRangeException()
            };

            damageValues = callback.GetMinMaxValues(player);
        }

        var areaHasDiagonals = Areas.ContainsKey(Direction.NorthEast);

        var direction = player.Direction;

        if (target != null)
        {
            direction = player.Location.DirectionTo(target.Location, areaHasDiagonals);

            if (direction == Direction.None)
                direction = player.Direction;
        }

        return new CombatParameter
        {
            DamageType = ((CombatType)combatType).ToDamageType(),
            Effect = (EffectT)effect,
            ShootType = (Domain.Common.Item.ShootType)shootType,
            MinDamage = (ushort)damageValues.Min,
            MaxDamage = (ushort)damageValues.Max,
            Range = 7,
            Area = Areas.Count != 0 ? Areas[direction] : null,
            Conditions = Conditions
        };
    }
}